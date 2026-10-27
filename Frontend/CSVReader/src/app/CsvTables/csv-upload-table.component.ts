import { Component, ChangeDetectorRef, AfterViewInit  } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ButtonModule } from 'primeng/button';
import { IconFieldModule } from 'primeng/iconfield';
import { InputIconModule } from 'primeng/inputicon';
import { MultiSelectModule } from 'primeng/multiselect';
import { InputTextModule } from 'primeng/inputtext';
import { SelectModule } from 'primeng/select';
import { FileUploadModule } from 'primeng/fileupload';
import { TableModule } from 'primeng/table';
import { Router } from '@angular/router';

// Interface pour les colonnes des fichiers
export interface CsvColumn {
  field: string;
  header: string;
}

// Interface pour la réponse de l'API
interface CsvFileResponse {
  id: number;
  name: string;
  originalFileName: string;
  uploadedAt: string;
  sizeBytes: number;
  headers: string[];
  rows: Record<string, string>[];
}

// Déclaration du composant
@Component({
  selector: 'app-csv-upload-table',
  standalone: true,
  imports: [
    CommonModule, FormsModule,
    FileUploadModule, ButtonModule,
    IconFieldModule, InputIconModule,
    MultiSelectModule, TableModule,
    InputTextModule, SelectModule
  ],
  templateUrl: './csv-upload-table.component.html',
  styleUrl:    './csv-upload-table.component.css'
})



export class CsvUploadTableComponent implements AfterViewInit {
  private readonly apiUrl = 'http://localhost:5191/api/Csv'; // URL de l'API

  data1: any[] = []; // données du tableau 1
  data1id: number | null = null; // identifiant du tableau 1
  data2: any[] = []; // données du tableau 2
  data2id: number | null = null; // identifiant du tableau 2
  columns1: CsvColumn[] = []; // colonnes du tableau 1
  columns2: CsvColumn[] = []; // colonnes du tableau 2
  selectedColumns1: CsvColumn[] = []; // colonnes sélectionnées du tableau 1
  selectedColumns2: CsvColumn[] = []; // colonnes sélectionnées du tableau 2
  selectedColumnsDiff : CsvColumn[] = []; // colonnes sélectionnées du tableau de comparaison

  keyColumn = ''; // colonne clé pour comparer
  allColumns: CsvColumn[] = []; // toutes les colonnes des fichiers
  commonColumns: string[] = []; // colonnes communes des fichiers
  diffRows: any[] = []; // lignes différentes entre les fichiers
  allDiffRows: any[] = [];  // toutes les lignes différentes entre les fichiers
  showDiffOnly = false; // booléen pour affichier seulement les différences
  diffCount = 0; // nombre de différences
  allKeysSize = 0;  // nombre de colonnes
  isLoading = false; // booléen pour le chargement de la page
  errorMessage = ''; // message d'erreur
  fileName1 = ''; // nom du premier fichier 
  fileName2 = ''; // nom du second fichier
  fileName = ''; // nom du dernier fichier uploadé

  private map2 = new Map<string, any>();

  // Constructeur du composant
  constructor(private http: HttpClient, private cdr: ChangeDetectorRef, private router: Router) {}

  asInputEl = (e: Event) => e.target as HTMLInputElement;

  get filterFields1():string[] { return this.columns1.map(c => c.field); }
  get filterFields2():string[] { return this.columns2.map(c => c.field); }

  // initialisation
  ngAfterViewInit() {
    this.data1 = [];
    this.data1id = null;
    this.data2 = [];
    this.data2id = null;

  }

  // upload d'un fichier, avec l'index du tableau en argument
  onUpload(event: any, tableIndex: 1 | 2) {
    const file: File = event.files[0];
    this.fileName = file.name;
    console.log(`Uploading file: ${file.name} (${file.size} bytes)`);
    const formData = new FormData();
    formData.append('file', file);

    this.isLoading = true;
    this.errorMessage = '';

    // requête pour poster le fichier à l'API
    this.http.post<CsvFileResponse>(this.apiUrl, formData).subscribe({
      next: (response) => {

        this.loadCsvData(response, tableIndex);
        this.data1id = tableIndex === 1 ? response.id : this.data1id;
        this.fileName1 = tableIndex === 1 ? response.originalFileName : this.fileName1;
        this.data2id = tableIndex === 2 ? response.id : this.data2id;
        this.fileName2 = tableIndex === 2 ? response.originalFileName : this.fileName2;
        this.isLoading = false;
      },
      error: (err) => {
        this.errorMessage = err.error ?? 'Erreur lors de l\'upload.';
        this.isLoading = false;
      }
    });
  }

  // suppresion d'un fichier, avec l'index du tableau en argument
  deleteCsv(tableIndex: 1 | 2) {
    const data = tableIndex === 1 ? this.data1 : this.data2;
    if (data.length === 0) {
      this.errorMessage = 'Aucun fichier à supprimer.';
      return;
    }
    this.errorMessage = '';
    // si le fichier correspond au premier tableau on supprime le premier fichier et on le remplace par le second
    if (tableIndex === 1) {
      this.http.delete(`${this.apiUrl}/${this.data1id}`).subscribe({
        next: () => {
          this.data1 = this.data2;
          this.data1id = this.data2id;
          this.fileName1 = this.fileName2;
          this.data2 = [];
          this.data2id = null;
          
        }
      });
    // sinon on supprime le second fichier
    } else {
      this.http.delete(`${this.apiUrl}/${this.data2id}`).subscribe({
        next: () => {
          this.data2 = [];
          this.data2id = null;
        }
      });
    }
  }

  // chargement des données des fichiers, avec la réponse de la requête et l'index du tableau en argument
  private loadCsvData(response: CsvFileResponse, tableIndex: 1 | 2) {
    console.log('response:', response);
    const columns: CsvColumn[] = response.headers.map(h => ({ field: h, header: h }));
    const data = response.rows;

    // si la réponse de la requête existe et qu'il s'agit du premier tableau, charger les données correspondantes 
    // dans les variables du premier tableau
      if (response.originalFileName && tableIndex === 1) {
        this.columns1 = columns;
        this.selectedColumns1 = [...columns];
        this.data1 = data;
      // si la réponse de la requête existe et qu'il s'agit du second tableau, charger les données correspondates
      // dans les variables du second tableau
      } else if (response.originalFileName && tableIndex === 2) {
        this.columns2 = columns;
        this.selectedColumns2 = [...columns];
        this.data2 = data;
      }
    
      // si les deux données des fichiers ont correctement été chargées, on construit le tableau de comparaison
      if (this.data1.length && this.data2.length) {
        const fields1 = this.columns1.map(c => c.field);
        const fields2 = this.columns2.map(c => c.field);

        this.commonColumns = fields1.filter(f => fields2.includes(f));
        this.allKeysSize   = this.commonColumns.length;

        const allFields = [...new Set([...fields1, ...fields2])];
        this.allColumns = allFields.map(f => ({ field: f, header: f }));

        this.keyColumn = this.commonColumns.length > 0 ? this.commonColumns[0] : '';
        
        this.diffRows  = [];
        this.diffCount = 0;
        this.map2.clear();
        this.buildDiff();
      }
    this.cdr.detectChanges();
  }

  // construction du tableau de comparaison
  buildDiff() {
    if (!this.keyColumn) return;

    const map1 = new Map(this.data1.map(r => [r[this.keyColumn], r]));
    this.map2  = new Map(this.data2.map(r => [r[this.keyColumn], r]));
    const allKeys = new Set([...map1.keys(), ...this.map2.keys()]);

    this.allDiffRows = Array.from(allKeys).map(key => {
      const r1 = map1.get(key);
      const r2 = this.map2.get(key);
      const merged: any = {};

      merged.__source = r1 && r2 ? 'both' : r1 ? 'only1' : 'only2';
      merged.__diffs  = new Set<string>();

      this.allColumns.forEach(({ field }) => {
        const v1 = r1?.[field];
        const v2 = r2?.[field];
        merged[field] = r1 ? (v1 ?? '') : (v2 ?? '');
        if (r1 && r2 && v1 !== v2) merged.__diffs.add(field);
      });
      return merged;
    });
    this.selectedColumnsDiff = [...this.allColumns];

    this.diffCount = this.allDiffRows.filter(
      r => r.__source !== 'both' || r.__diffs.size > 0
    ).length;

    this.showDiffOnly = false;
    this.diffRows = [...this.allDiffRows];
  }

  // afficher seulement les différences/tout dans le tableau de comparaison
  toggleDiffOnly() {
    this.showDiffOnly = !this.showDiffOnly;
    this.diffRows = this.showDiffOnly
      ? this.allDiffRows.filter(r => r.__source !== 'both' || r.__diffs.size > 0)
      : [...this.allDiffRows];
  }
  
  // modifier le fond des cases du tableau de comparaison
  getCellClass(row: any, col: CsvColumn): string {
    if (row.__source === 'only1')    return 'only-in-1';
    if (row.__source === 'only2')    return 'only-in-2';
    if (row.__diffs?.has(col.field)) return 'diff-cell';
    return '';
  }

  // obtenir la seconde valeur pour les valeurs différentes
  getCell2Value(row: any, col: CsvColumn): string | null {
    if (row.__source !== 'both' || !row.__diffs?.has(col.field)) return null;
    const key = row[this.keyColumn];
    return this.map2.get(key)?.[col.field] ?? null;
  }

  // navigation entre les pages
  navigateToCreateEP() {
    this.router.navigate(['/create-endpoint']);
  }
}