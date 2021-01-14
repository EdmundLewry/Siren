import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { ChannelEditDialog } from './channel-edit-dialog.component';

describe('ChannelEditDialogComponent', () => {
  let component: ChannelEditDialog;
  let fixture: ComponentFixture<ChannelEditDialog>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ ChannelEditDialog ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(ChannelEditDialog);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
