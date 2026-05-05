import { Component, signal } from '@angular/core';

@Component({
  selector: 'app-root',
  templateUrl: './app.html',
})
export class App {
  counter = signal(0);

  increment() {
    this.counter.update(n => n + 1);
  }
}
