import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { TransmissionListEventEditDialog } from './transmission-list-event-edit-dialog.component';

describe('CreateEventDialogComponent', () => {
  let component: TransmissionListEventEditDialog;
  let fixture: ComponentFixture<TransmissionListEventEditDialog>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ TransmissionListEventEditDialog ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(TransmissionListEventEditDialog);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
