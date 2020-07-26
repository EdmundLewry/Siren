import { TestBed } from '@angular/core/testing';

import { AutomationService } from './automation.service';

describe('AutomationService', () => {
  beforeEach(() => TestBed.configureTestingModule({}));

  it('should be created', () => {
    const service: AutomationService = TestBed.get(AutomationService);
    expect(service).toBeTruthy();
  });
});
