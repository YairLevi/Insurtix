import { TestBed } from '@angular/core/testing';
import { App } from './app';

describe('App', () => {
  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [App],
    }).compileComponents();
  });

  it('should create the app', () => {
    const fixture = TestBed.createComponent(App);
    const app = fixture.componentInstance;
    expect(app).toBeTruthy();
  });

  it('should increment counter on button click', () => {
    const fixture = TestBed.createComponent(App);
    const app = fixture.componentInstance;
    app.increment();
    expect(app.counter()).toBe(1);
  });
});
