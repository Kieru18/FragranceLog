import {
  bootstrapApplication,
  provideNativeScriptHttpClient,
  provideNativeScriptNgZone,
  provideNativeScriptRouter,
  runNativeScriptAngularApp,
} from '@nativescript/angular';
import { provideExperimentalZonelessChangeDetection } from '@angular/core';
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


function navigateOrQueue(token: string) {
  try {
    const router = (global as any).__routerExt as RouterExtensions | undefined;
    if (router) {
      setTimeout(() => router.navigate(['/shared', token]), 0);
    } else {
      pendingSharedToken = token;
    }
  } catch {
    pendingSharedToken = token;
  }
}

if (Application.android) {
  const androidApp = Application.android as AndroidApplication;

  androidApp.on(AndroidApplication.activityCreatedEvent, (args) => {
    const token = extractSharedToken(args.activity?.getIntent?.());
    if (token) {
      pendingSharedToken = token;
    }
  });

  androidApp.on(AndroidApplication.activityNewIntentEvent, (args) => {
    try {
      args.activity?.setIntent?.(args.intent);
    } catch {
      //
    }

    const token = extractSharedToken(args.intent);
    if (token) {
      navigateOrQueue(token);
    }
  });
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
      const routerExt = appRef.injector.get(RouterExtensions);
      (global as any).__routerExt = routerExt;

      if (pendingSharedToken) {
        const token = pendingSharedToken;
        pendingSharedToken = null;

        setTimeout(() => routerExt.navigate(['/shared', token]), 0);
      }

      return appRef;
    });
  },
});
