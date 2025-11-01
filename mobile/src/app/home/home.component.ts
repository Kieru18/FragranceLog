import { Component, NO_ERRORS_SCHEMA } from '@angular/core';
import { registerElement } from '@nativescript/angular';
import { RouterLink } from '@angular/router';
import { Image } from '@nativescript/core';

registerElement('Image', () => Image);

@Component({
  standalone: true,
  selector: 'ns-home',
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.css'],
  imports: [RouterLink],
  schemas: [NO_ERRORS_SCHEMA],
})
export class HomeComponent {}
