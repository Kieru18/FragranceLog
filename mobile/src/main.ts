import {
  bootstrapApplication,
  provideNativeScriptHttpClient,
  provideNativeScriptNgZone,
  provideNativeScriptRouter,
  runNativeScriptAngularApp,
} from '@nativescript/angular';
import { provideExperimentalZonelessChangeDetection } from '@angular/core';
import { HTTP_INTERCEPTORS, withInterceptorsFromDi } from '@angular/common/http';
import { Application } from '@nativescript/core';

import { routes } from './app/app.routes';
import { AppComponent } from './app/app.component';
import { AuthInterceptor } from './app/services/auth.interceptor';
import { RouterExtensions } from '@nativescript/angular';

/**
 * Disable zone by setting this to true
 * Then also adjust polyfills.ts (see note there)
 */
const EXPERIMENTAL_ZONELESS = false;

/**
 * Holds shared-list token until Angular router is ready
 */
let pendingSharedToken: string | null = null;

/**
 * Capture Android deep links (warm start)
 */
Application.android.on(
  Application.android.activityNewIntentEvent,
  (args) => {
    const intent = args.intent;
    const data = intent?.getData?.();

    if (!data) return;

    const scheme = data.getScheme();
    const host = data.getHost();
    const token = data.getLastPathSegment();

    if (scheme === 'fragrancelog' && host === 'shared' && token) {
      pendingSharedToken = token;
    }
  }
);

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
    });
  },
});

/**
 * Handle cold start deep link after Angular bootstrap
 */
Application.on(Application.launchEvent, () => {
  if (!pendingSharedToken) return;

  try {
    const router = (global as any).ngRef
      ?.injector
      ?.get(RouterExtensions);

    if (router) {
      router.navigate(['/shared', pendingSharedToken]);
      pendingSharedToken = null;
    }
  } catch {
    //
  }
});
