import { Injectable } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { requestPermissions, takePicture } from '@nativescript/camera';
import { Application, File } from '@nativescript/core';
import { PerfumeRecognitionService } from '../services/perfumerecognition.service';
import { PerfumeRecognitionLoadingComponent } from '../recognition/loading/perfumerecognitionloading.component';
import { PerfumeRecognitionModalComponent } from '../recognition/modal/perfumerecognitionmodal.component';

@Injectable({ providedIn: 'root' })
export class RecognitionFlowService {

  constructor(
    private readonly api: PerfumeRecognitionService
  ) {}

  async start(): Promise<void> {
    console.log('[REC] start() ENTER');

    let imageBase64: string;

    try {
      console.log('[REC] taking photo (base64)');
      imageBase64 = await this.takePhotoBase64();
      console.log('[REC] base64 length =', imageBase64.length);
    } catch (e) {
      console.log('[REC] takePhotoBase64 FAILED', e);
      return;
    }

    console.log('[REC] opening loading modal');
    (global as any).openRecognitionModal(
      PerfumeRecognitionLoadingComponent
    );

    try {
      console.log('[REC] calling API /perfume-recognition');

      const results = await firstValueFrom(
        this.api.recognize({
          imageBase64: imageBase64,
          topK: 5
        })
      );

      console.log('[REC] API SUCCESS, results:', results);

      console.log('[REC] closing loading modal');
      (global as any).closeRecognitionLoading?.();

      console.log('[REC] opening results modal');
      (global as any).openRecognitionModal(
        PerfumeRecognitionModalComponent,
        { results }
      );
    } catch (e) {
      console.log('[REC] API FAILED', e);
      console.log('[REC] closing loading modal (error)');
      (global as any).closeRecognitionLoading?.();
    }

    console.log('[REC] start() EXIT');
  }

  private async takePhotoBase64(): Promise<string> {
    console.log('[REC] requestPermissions()');
    await requestPermissions();

    console.log('[REC] takePicture()');
    const asset = await takePicture({
      saveToGallery: false
    });

    if (!asset) {
      throw new Error('Camera cancelled');
    }

    const filePath = asset.android || asset.ios;
    console.log('[REC] camera file path =', filePath);

    if (!filePath) {
      throw new Error('No file path from camera');
    }

    console.log('[REC] file exists =', File.exists(filePath));

    const file = File.fromPath(filePath);
    const bytes = file.readSync();
    console.log('[REC] bytes length =', bytes.length);

    let base64: string;

    if (Application.android) {
      base64 = android.util.Base64.encodeToString(
        bytes,
        android.util.Base64.NO_WRAP
      );
    } else {
      throw new Error('iOS not implemented yet');
    }

    console.log('[REC] base64 generated, length =', base64.length);
    return base64;
  }
}
