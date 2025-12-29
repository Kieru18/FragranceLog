import {
  bootstrapApplication,
  provideNativeScriptHttpClient,
  provideNativeScriptNgZone,
  provideNativeScriptRouter,
  runNativeScriptAngularApp,
} from '@nativescript/angular';
import {
  provideExperimentalZonelessChangeDetection,
  ApplicationRef
} from '@angular/core';
import { HTTP_INTERCEPTORS, withInterceptorsFromDi } from '@angular/common/http';
import { Application, AndroidApplication } from '@nativescript/core';

import { routes } from './app/app.routes';
import { AppComponent } from './app/app.component';
import { AuthInterceptor } from './app/services/auth.interceptor';
import { RouterExtensions } from '@nativescript/angular';

const EXPERIMENTAL_ZONELESS = false;

let pendingSharedToken: string | null = null;

function tryExtractSharedToken(intent?: android.content.Intent | null) {
  const data = intent?.getData?.();
  if (!data) return;

  const scheme = data.getScheme();
  const host = data.getHost();
  const token = data.getLastPathSegment();

  if (scheme === 'fragrancelog' && host === 'shared' && token) {
    pendingSharedToken = token;
  }
}

if (Application.android) {
  const androidApp = Application.android as AndroidApplication;

  // cold start
  tryExtractSharedToken(androidApp.startActivity?.getIntent());

  // warm start
  androidApp.on(
    AndroidApplication.activityNewIntentEvent,
    (args) => tryExtractSharedToken(args.intent)
  );
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
          multi: true
        }
      ],
    }).then((appRef: ApplicationRef) => {
      if (pendingSharedToken) {
        const router = appRef.injector.get(RouterExtensions);
        router.navigate(['/shared', pendingSharedToken]);
        pendingSharedToken = null;
      }

      return appRef;
    });
  },
});
