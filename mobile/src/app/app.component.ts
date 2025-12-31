import { Component, NO_ERRORS_SCHEMA } from '@angular/core';
import { PageRouterOutlet } from '@nativescript/angular';
import { Application, AndroidApplication, AndroidActivityBackPressedEventData } from '@nativescript/core';
import { RouterExtensions } from '@nativescript/angular';

@Component({
  selector: 'ns-app',
  templateUrl: './app.component.html',
  imports: [PageRouterOutlet],
  schemas: [NO_ERRORS_SCHEMA],
})
export class AppComponent {
  constructor() {
    if (Application.android) {
      Application.android.on(Application.android.activityBackPressedEvent, (args: AndroidActivityBackPressedEventData) => {
        const routerExtensions = (global as any).ngRef?.injector?.get(RouterExtensions);
        const currentRoute = routerExtensions?.router?.url || '';
        
        if (currentRoute.includes('/shared/')) {
          args.cancel = true;
          return;
        }
        
        if (routerExtensions && routerExtensions.canGoBack()) {
          args.cancel = true;
          routerExtensions.back();
        }
      });
    }
  }
}
