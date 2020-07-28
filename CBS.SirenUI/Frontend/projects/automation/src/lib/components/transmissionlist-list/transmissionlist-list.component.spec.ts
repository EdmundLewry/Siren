import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { TransmissionlistListComponent } from './transmissionlist-list.component';

describe('TransmissionlistListComponent', () => {
  let component: TransmissionlistListComponent;
  let fixture: ComponentFixture<TransmissionlistListComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ TransmissionlistListComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(TransmissionlistListComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
