import { ComponentFixture, TestBed } from '@angular/core/testing';

import { CreateEndpoint } from './create-endpoint';

describe('CreateEndpoint', () => {
  let component: CreateEndpoint;
  let fixture: ComponentFixture<CreateEndpoint>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [CreateEndpoint],
    }).compileComponents();

    fixture = TestBed.createComponent(CreateEndpoint);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
