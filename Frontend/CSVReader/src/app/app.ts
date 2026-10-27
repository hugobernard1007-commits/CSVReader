import { Component, signal } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { CsvUploadTableComponent } from './CsvTables/csv-upload-table.component';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App {
  protected readonly title = signal('CSVReader');
}
