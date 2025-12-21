import { Component, ElementRef, NO_ERRORS_SCHEMA, OnInit, ViewChild, AfterViewInit, ChangeDetectorRef } from '@angular/core';
import { NativeScriptCommonModule, NativeScriptFormsModule } from '@nativescript/angular';
import { RouterModule } from '@angular/router';
import { finalize } from 'rxjs/operators';
import { Page, Screen, Utils, View } from '@nativescript/core';
import { HttpErrorResponse } from '@angular/common/http';
import { PerfumeListService } from '../../services/perfumelist.service';
import { PerfumeListOverviewDto } from '../../models/perfumelistoverview.dto';
import { FooterComponent } from '~/app/footer/footer.component';

type PreviewSlot = { path: string | null };
type DialogMode = 'create' | 'rename' | 'delete';

@Component({
  standalone: true,
  selector: 'app-lists-overview',
  templateUrl: './listsoverview.component.html',
  imports: [NativeScriptCommonModule, RouterModule, FooterComponent, NativeScriptFormsModule],
  schemas: [NO_ERRORS_SCHEMA]
})
export class ListsOverviewComponent implements OnInit, AfterViewInit {
  loading = false;
  error: string | null = null;
  items: PerfumeListOverviewDto[] = [];
  
  dialog = {
    visible: false,
    mode: 'create' as DialogMode,
    listId: null as number | null,
    name: ''
  };

  menu = {
    visible: false,
    listId: null as number | null,
    name: ''
  };

  private readonly screenHeight = Screen.mainScreen.heightDIPs;
  private isAnimating = false;

  @ViewChild('dialogBackdrop', { static: false }) dialogBackdropRef?: ElementRef<View>;
  @ViewChild('dialogPanel', { static: false }) dialogPanelRef?: ElementRef<View>;
  @ViewChild('menuBackdrop', { static: false }) menuBackdropRef?: ElementRef<View>;
  @ViewChild('menuPanel', { static: false }) menuPanelRef?: ElementRef<View>;

  constructor(
    private readonly lists: PerfumeListService,
    private page: Page,
    private cdRef: ChangeDetectorRef
  ) {
    this.page.actionBarHidden = true;
  }

  ngOnInit(): void {
    this.load();
  }

  ngAfterViewInit(): void {
    setTimeout(() => {
      this.cdRef.detectChanges();
    }, 0);
  }

  load(): void {
    this.loading = true;
    this.error = null;
    this.lists.getListsOverview()
      .pipe(finalize(() => (this.loading = false)))
      .subscribe({
        next: (data: PerfumeListOverviewDto[]) => {
          this.items = (data ?? []).slice().sort((a, b) => {
            const sys = (a.isSystem === b.isSystem) ? 0 : (a.isSystem ? -1 : 1);
            if (sys !== 0) return sys;
            return (a.name ?? '').localeCompare(b.name ?? '');
          });
        },
        error: (err: HttpErrorResponse) => {
          console.log(err);
          this.error = 'Failed to load lists.';
        }
      });
  }

  openCreateDialog(): void {
    Utils.dismissSoftInput();
    this.menu.visible = false;
    this.dialog = {
      visible: true,
      mode: 'create',
      listId: null,
      name: ''
    };
    setTimeout(() => {
      this.animateDialogIn();
    }, 10);
  }

  openListMenu(item: PerfumeListOverviewDto): void {
    if (item.isSystem) return;
    this.dialog.visible = false;
    this.menu = {
      visible: true,
      listId: item.perfumeListId,
      name: item.name ?? ''
    };
    setTimeout(() => {
      this.animateMenuIn();
    }, 10);
  }

  closeMenu(): void {
    if (!this.menu.visible || this.isAnimating) return;
    this.animateMenuOut().then(() => {
      this.menu.visible = false;
    });
  }

  chooseRename(): void {
    if (this.isAnimating) return;
    
    const id = this.menu.listId;
    const name = this.menu.name;
    
    this.animateMenuOut().then(() => {
      this.menu.visible = false;
      
      if (!id) return;
      
      setTimeout(() => {
        this.dialog = {
          visible: true,
          mode: 'rename',
          listId: id,
          name: name ?? ''
        };
        
        setTimeout(() => {
          this.animateDialogIn();
        }, 10);
      }, 10);
    });
  }

  chooseDelete(): void {
    if (this.isAnimating) return;
    
    const id = this.menu.listId;
    const name = this.menu.name;
    
    this.animateMenuOut().then(() => {
      this.menu.visible = false;
      
      if (!id) return;
      
      setTimeout(() => {
        this.dialog = {
          visible: true,
          mode: 'delete',
          listId: id,
          name: name ?? ''
        };
        
        setTimeout(() => {
          this.animateDialogIn();
        }, 10);
      }, 10);
    });
  }

  closeDialog(): void {
    if (!this.dialog.visible || this.isAnimating) return;
    Utils.dismissSoftInput();
    this.animateDialogOut().then(() => {
      this.dialog.visible = false;
    });
  }

  confirmDialog(): void {
    const mode = this.dialog.mode;
    const id = this.dialog.listId;
    const name = this.dialog.name.trim();
    
    Utils.dismissSoftInput();
    
    if (mode !== 'delete' && !name) return;
    
    this.animateDialogOut().then(() => {
      this.dialog.visible = false;
      
      if (mode === 'create') {
        this.loading = true;
        this.error = null;
        this.lists.createList(name)
          .pipe(finalize(() => (this.loading = false)))
          .subscribe({
            next: () => this.load(),
            error: (err: HttpErrorResponse) => {
              console.log(err);
              this.error = 'Failed to create list.';
            }
          });
        return;
      }
      
      if (mode === 'rename' && id) {
        this.loading = true;
        this.error = null;
        this.lists.renameList(id, name)
          .pipe(finalize(() => (this.loading = false)))
          .subscribe({
            next: () => this.load(),
            error: (err: HttpErrorResponse) => {
              console.log(err);
              this.error = 'Failed to rename list.';
            }
          });
        return;
      }
      
      if (mode === 'delete' && id) {
        this.loading = true;
        this.error = null;
        this.lists.deleteList(id)
          .pipe(finalize(() => (this.loading = false)))
          .subscribe({
            next: () => this.load(),
            error: (err: HttpErrorResponse) => {
              console.log(err);
              this.error = 'Failed to delete list.';
            }
          });
      }
    });
  }

  private animateDialogIn(): void {
    if (this.isAnimating) return;
    this.isAnimating = true;
    
    setTimeout(() => {
      const backdrop = this.dialogBackdropRef?.nativeElement;
      const panel = this.dialogPanelRef?.nativeElement;

      if (!backdrop || !panel) {
        this.isAnimating = false;
        return;
      }

      backdrop.opacity = 0;
      panel.opacity = 0;
      (panel as any).scaleX = 0.96;
      (panel as any).scaleY = 0.96;

      backdrop.animate({
        opacity: 0.6,
        duration: 200,
        curve: 'easeOut'
      }).then(() => {
        this.isAnimating = false;
      }).catch(() => {
        this.isAnimating = false;
      });

      panel.animate({
        opacity: 1,
        scale: { x: 1, y: 1 },
        duration: 220,
        curve: 'easeOut'
      });
    }, 0);
  }

  private animateDialogOut(): Promise<void> {
    if (this.isAnimating) return Promise.resolve();
    this.isAnimating = true;
    
    const backdrop = this.dialogBackdropRef?.nativeElement;
    const panel = this.dialogPanelRef?.nativeElement;

    if (!backdrop || !panel) {
      this.isAnimating = false;
      return Promise.resolve();
    }

    const p1 = backdrop.animate({
      opacity: 0,
      duration: 160,
      curve: 'easeIn'
    });

    const p2 = panel.animate({
      opacity: 0,
      scale: { x: 0.96, y: 0.96 },
      duration: 160,
      curve: 'easeIn'
    });

    return Promise.all([p1, p2]).then(() => {
      this.isAnimating = false;
    }).catch(() => {
      this.isAnimating = false;
    });
  }

  private animateMenuIn(): void {
    if (this.isAnimating) return;
    this.isAnimating = true;
    
    setTimeout(() => {
      const backdrop = this.menuBackdropRef?.nativeElement;
      const panel = this.menuPanelRef?.nativeElement;

      if (!backdrop || !panel) {
        this.isAnimating = false;
        return;
      }

      backdrop.opacity = 0;
      panel.opacity = 0;
      panel.translateY = 12;
      (panel as any).scaleX = 0.98;
      (panel as any).scaleY = 0.98;

      backdrop.animate({
        opacity: 0.6,
        duration: 160,
        curve: 'easeOut'
      }).then(() => {
        this.isAnimating = false;
      }).catch(() => {
        this.isAnimating = false;
      });

      panel.animate({
        opacity: 1,
        translate: { x: 0, y: 0 },
        scale: { x: 1, y: 1 },
        duration: 180,
        curve: 'easeOut'
      });
    }, 0);
  }

  private animateMenuOut(): Promise<void> {
    if (this.isAnimating) return Promise.resolve();
    this.isAnimating = true;
    
    const backdrop = this.menuBackdropRef?.nativeElement;
    const panel = this.menuPanelRef?.nativeElement;

    if (!backdrop || !panel) {
      this.isAnimating = false;
      return Promise.resolve();
    }

    const p1 = backdrop.animate({
      opacity: 0,
      duration: 140,
      curve: 'easeIn'
    });

    const p2 = panel.animate({
      opacity: 0,
      translate: { x: 0, y: 12 },
      scale: { x: 0.98, y: 0.98 },
      duration: 140,
      curve: 'easeIn'
    });

    return Promise.all([p1, p2]).then(() => {
      this.isAnimating = false;
    }).catch(() => {
      this.isAnimating = false;
    });
  }

  previewSlots(item: PerfumeListOverviewDto): PreviewSlot[] {
    const imgs = (item.previewImages ?? []).filter(x => !!x);
    const slots: PreviewSlot[] = [];
    for (let i = 0; i < 4; i++) {
      slots.push({ path: imgs[i] ?? null });
    }
    return slots;
  }

  trackByListId(_: number, item: PerfumeListOverviewDto): number {
    return item.perfumeListId;
  }

  trackByIndex(i: number): number {
    return i;
  }
}
