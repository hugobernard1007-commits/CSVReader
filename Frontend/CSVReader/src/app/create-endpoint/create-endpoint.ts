import { Component, ChangeDetectorRef  } from '@angular/core';
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


// Interface pour les colonnes des fichiers
export interface CsvColumn {
  field: string;
  header: string;
}

// Interface pour la réponse après publication du fichier
interface PublishResponse {
  id: number;
  name: string;
  originalFileName: string;
}

// Déclaration du composant
@Component({
  selector: 'app-create-endpoint',
  imports: [ CommonModule, FormsModule,
    FileUploadModule, ButtonModule,
    IconFieldModule, InputIconModule,
    MultiSelectModule, TableModule,
    InputTextModule, SelectModule],
  templateUrl: './create-endpoint.html',
  styleUrl: './create-endpoint.css',
})

export class CreateEndpoint {
  private readonly apiUrl = 'http://localhost:5191/api/Csv'; // URL de l'API

  isLoading = false; // booléen pour le chargement
  errorMessage = '';  // message d'erreur
  generatedUrl = ''; // URL de l'endpoint crée

  // constructeur du composant
  constructor(private http: HttpClient, private cdr: ChangeDetectorRef) {}

  // upload d'un fichier
  onUpload(event: any) {
    const file: File = event.files[0];
    const formData = new FormData();
    formData.append('file', file);

    this.isLoading    = true;
    this.errorMessage = '';
    this.generatedUrl = '';

    //requête pour poster un fichier, qui renvoie une réponse (nom du fichier) pour créer le lien de l'endpoint
    this.http.post<PublishResponse>(this.apiUrl, formData).subscribe({
      next: (response) => {
        this.generatedUrl = `https://localhost:7056/api/data/${response.name}`;
        this.isLoading    = false;
        this.cdr.detectChanges();
      },
      error: (err) => {
        this.errorMessage = err.error ?? 'Erreur lors de l\'upload.';
        this.isLoading    = false;
        this.cdr.detectChanges();
      }
    });
  }

  // fonction pour copier l'URL générée
  copyUrl() {
    navigator.clipboard.writeText(this.generatedUrl);
  }
}
