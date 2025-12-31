import {
  bootstrapApplication,
  provideNativeScriptHttpClient,
  provideNativeScriptNgZone,
  provideNativeScriptRouter,
  runNativeScriptAngularApp,
} from '@nativescript/angular';
import { provideExperimentalZonelessChangeDetection, ApplicationRef } from '@angular/core';
import { HTTP_INTERCEPTORS, withInterceptorsFromDi } from '@angular/common/http';
import { Application, AndroidApplication } from '@nativescript/core';

import { routes } from './app/app.routes';
import { AppComponent } from './app/app.component';
import { AuthInterceptor } from './app/services/auth.interceptor';
import { RouterExtensions } from '@nativescript/angular';

const EXPERIMENTAL_ZONELESS = false;

let pendingSharedToken: string | null = null;

function extractSharedToken(intent?: android.content.Intent | null): string | null {
  const data = intent?.getData?.();
  if (!data) return null;

  const scheme = data.getScheme?.();
  const host = data.getHost?.();
  const token = data.getLastPathSegment?.();

  if (scheme === 'fragrancelog' && host === 'shared' && token) {
    return String(token);
  }

  return null;
}

function handleDeepLinkNavigation(token: string) {
  const appRef = (global as any).ngRef as any | undefined;
  if (!appRef?.injector) {
    pendingSharedToken = token;
    return;
  }

  const router = appRef.injector.get(RouterExtensions);
  
  setTimeout(() => {
    router.navigate(['/shared', token], {
      clearHistory: true,
      animated: false,
      transition: undefined
    });
  }, 0);
}

if (Application.android) {
  const androidApp = Application.android as AndroidApplication;

  if (!(global as any).__flDeepLinkInstalled) {
    (global as any).__flDeepLinkInstalled = true;

    const coldToken = extractSharedToken(androidApp.startActivity?.getIntent?.());
    if (coldToken) {
      pendingSharedToken = coldToken;
    }

    androidApp.on(AndroidApplication.activityNewIntentEvent, (args) => {
      const token = extractSharedToken(args.intent);
      if (!token) return;

      const appRef = (global as any).ngRef as any | undefined;
      if (appRef?.injector) {
        setTimeout(() => handleDeepLinkNavigation(token), 100);
      } else {
        pendingSharedToken = token;
      }
    });
  }
}

runNativeScriptAngularApp({
  appModuleBootstrap: () => {
    return bootstrapApplication(AppComponent, {
      providers: [
        provideNativeScriptHttpClient(withInterceptorsFromDi()),
        provideNativeScriptRouter(routes),
        EXPERIMENTAL_ZONELESS
          ? provideExperimentalZonelessChangeDetection()
          : provideNativeScriptNgZone(),
        {
          provide: HTTP_INTERCEPTORS,
          useClass: AuthInterceptor,
          multi: true,
        },
      ],
    }).then((appRef) => {
      (global as any).ngRef = appRef;

      if (pendingSharedToken) {
        const token = pendingSharedToken;
        pendingSharedToken = null;
        setTimeout(() => handleDeepLinkNavigation(token), 100);
      }

      return appRef;
    });
  },
});
