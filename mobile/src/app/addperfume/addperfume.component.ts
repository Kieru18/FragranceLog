import { Component, NO_ERRORS_SCHEMA } from '@angular/core';
import {
  NativeScriptCommonModule,
  NativeScriptFormsModule,
  RouterExtensions
} from '@nativescript/angular';
import { Page, ImageSource } from '@nativescript/core';
import { PerfumeSuggestionService } from '../services/perfumesuggestion.service';
import { NoteTypeEnum } from '../enums/notetype.enum';
import { FooterComponent } from '../footer/footer.component';
import { SnackBar } from '@nativescript-community/ui-material-snackbar';
import * as ImagePicker from '@nativescript/imagepicker';

@Component({
  standalone: true,
  selector: 'app-add-perfume',
  templateUrl: './addperfume.component.html',
  imports: [
    NativeScriptCommonModule,
    NativeScriptFormsModule,
    FooterComponent
  ],
  schemas: [NO_ERRORS_SCHEMA]
})
export class AddPerfumeComponent {
  brand = '';
  name = '';
  imageUrl = '';
  comment = '';

  selectedImage?: ImageSource;
  selectedImageBase64?: string;

  groups: string[] = [];
  newGroup = '';

  topNotes: string[] = [];
  heartNotes: string[] = [];
  baseNotes: string[] = [];

  newTop = '';
  newHeart = '';
  newBase = '';

  submitting = false;

  private snackBar = new SnackBar();

  constructor(
    private service: PerfumeSuggestionService,
    private page: Page,
    private routerExtensions: RouterExtensions
  ) {
    this.page.actionBarHidden = true;
  }

  addGroup() {
    const v = this.newGroup.trim();
    if (!v) return;
    this.groups.push(v);
    this.newGroup = '';
  }

  addNote(type: NoteTypeEnum) {
    if (type === NoteTypeEnum.Top && this.newTop.trim()) {
      this.topNotes.push(this.newTop.trim());
      this.newTop = '';
    }
    if (type === NoteTypeEnum.Middle && this.newHeart.trim()) {
      this.heartNotes.push(this.newHeart.trim());
      this.newHeart = '';
    }
    if (type === NoteTypeEnum.Base && this.newBase.trim()) {
      this.baseNotes.push(this.newBase.trim());
      this.newBase = '';
    }
  }

  pickImage(): void {
    const context = ImagePicker.create({ mode: 'single' });

    context.authorize()
      .then(() => context.present())
      .then(selection => {
        if (!selection || selection.length === 0) return;

        const sel = selection[0];
        const asset = sel.asset;

        return ImageSource.fromAsset(asset);
      })
      .then(image => {
        if (!image) return;

        this.selectedImage = image;
        this.selectedImageBase64 = image.toBase64String('jpeg', 80);
      })
      .catch(err => {
        console.error('Image pick failed:', err);
      });
  }

  submit() {
    if (!this.brand.trim() || !this.name.trim() || this.submitting) return;

    this.submitting = true;
    const perfumeName = this.name.trim();

    this.service.submit({
      brand: this.brand.trim(),
      name: perfumeName,
      imageUrl: this.imageUrl?.trim() || undefined,
      imageBase64: this.selectedImageBase64,
      comment: this.comment?.trim() || undefined,
      groups: this.groups,
      noteGroups: [
        { type: NoteTypeEnum.Top, notes: this.topNotes },
        { type: NoteTypeEnum.Middle, notes: this.heartNotes },
        { type: NoteTypeEnum.Base, notes: this.baseNotes }
      ]
    }).subscribe({
      next: () => {
        this.submitting = false;

        this.routerExtensions.navigate(
          ['/'],
          {
            clearHistory: true,
            state: {
              snackbarMessage: `Perfume "${perfumeName}" submitted successfully!`
            },
            transition: { name: 'slideRight' }
          }
        );
      },
      error: (err) => {
        this.submitting = false;
        console.error(err);

        this.snackBar.simple(
          'Submission failed. Please try again later.',
          '#FFFFFF',
          '#B00020',
          2
        );
      }
    });
  }
}
