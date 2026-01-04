import {
  bootstrapApplication,
  provideNativeScriptHttpClient,
  provideNativeScriptNgZone,
  provideNativeScriptRouter,
  runNativeScriptAngularApp,
  ModalDialogService,
} from '@nativescript/angular';
import { provideExperimentalZonelessChangeDetection } from '@angular/core';
import { HTTP_INTERCEPTORS, withInterceptorsFromDi } from '@angular/common/http';
import { Application, AndroidApplication } from '@nativescript/core';

import { routes } from './app/app.routes';
import { AppComponent } from './app/app.component';
import { AuthInterceptor } from './app/services/auth.interceptor';

const EXPERIMENTAL_ZONELESS = false;

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

if (Application.android) {
  const androidApp = Application.android as AndroidApplication;

  if (!(global as any).__flDeepLinkInstalled) {
    (global as any).__flDeepLinkInstalled = true;

    const coldToken = extractSharedToken(androidApp.startActivity?.getIntent?.());
    if (coldToken) {
      (global as any).pendingSharedToken = coldToken;
    }

    androidApp.on(AndroidApplication.activityNewIntentEvent, (args) => {
      const token = extractSharedToken(args.intent);
      if (!token) return;
      (global as any).pendingSharedToken = token;
      if ((global as any).openSharedModal) {
        (global as any).openSharedModal(token);
      }
    });
  }
}

runNativeScriptAngularApp({
  appModuleBootstrap: () =>
    bootstrapApplication(AppComponent, {
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
        ModalDialogService
      ],
    }),
});
