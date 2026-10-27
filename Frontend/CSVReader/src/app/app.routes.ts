import { Routes } from '@angular/router';
import { CreateEndpoint } from './create-endpoint/create-endpoint';
import { CsvUploadTableComponent } from './CsvTables/csv-upload-table.component';

export const routes: Routes = [
    { path: 'create-endpoint', component: CreateEndpoint },
    { path: '', component: CsvUploadTableComponent },
];
