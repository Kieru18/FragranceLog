import { Injectable } from '@angular/core';
import {
  enableLocationRequest,
  getCurrentLocation,
  isEnabled
} from '@nativescript/geolocation';
import { ApplicationSettings } from '@nativescript/core';

export type LocationConsent = 'unknown' | 'granted' | 'denied';

@Injectable({ providedIn: 'root' })
export class LocationService {

  private readonly PERMISSION_KEY = 'location.permission.granted';
  private readonly DENIED_KEY = 'location.permission.denied';

  getConsent(): LocationConsent {
    if (ApplicationSettings.getBoolean(this.PERMISSION_KEY, false)) {
      return 'granted';
    }

    if (ApplicationSettings.getBoolean(this.DENIED_KEY, false)) {
      return 'denied';
    }

    return 'unknown';
  }

  async requestPermission(): Promise<boolean> {
  if (ApplicationSettings.getBoolean(this.PERMISSION_KEY, false)) {
    return true;
  }

  try {
    await enableLocationRequest(true, true);
    ApplicationSettings.setBoolean(this.PERMISSION_KEY, true);
    ApplicationSettings.remove(this.DENIED_KEY);
    return true;
  } catch {
    ApplicationSettings.setBoolean(this.DENIED_KEY, true);
    return false;
  }
}

  async getCoordinates(): Promise<{ lat: number; lng: number } | null> {
  const hasPermission = ApplicationSettings.getBoolean(this.PERMISSION_KEY, false);
  
  if (!hasPermission) {
    return null;
  }

  try {
    const isLocEnabled = await isEnabled();
    
    if (!isLocEnabled) {
      const granted = await this.requestPermission();
      if (!granted) return null;
    }

    const loc = await getCurrentLocation({
      desiredAccuracy: 300,
      timeout: 30000,
      maximumAge: 300000
    });

    if (!loc) {
      return null;
    }

    return {
      lat: loc.latitude,
      lng: loc.longitude
    };
  } catch {
    return null;
  }
}
}