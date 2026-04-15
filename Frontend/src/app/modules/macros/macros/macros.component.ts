import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

import {
  CardModule,
  ButtonModule,
  FormModule
} from '@coreui/angular';

interface Macro {
  id: number;
  key: string;
  text: string;
}

@Component({
  selector: 'app-macros',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    CardModule,
    ButtonModule,
    FormModule
  ],
  templateUrl: './macros.component.html',
  styleUrl: './macros.component.scss'
})
export class MacrosComponent {

  macros: Macro[] = [
    { id: 1, key: 'hola',      text: 'Hola! ¿En qué podemos ayudarte?' },
    { id: 2, key: 'horario',   text: 'Nuestro horario es de 9 a 18 hrs' },
    { id: 3, key: 'direccion', text: 'Estamos ubicados en Av. Principal 123' },
  ];

  newMacro: Omit<Macro, 'id'> = { key: '', text: '' };
  searchTerm = '';
  private nextId = 4;

  get filteredMacros(): Macro[] {
    const term = this.searchTerm.toLowerCase();
    if (!term) return this.macros;
    return this.macros.filter(
      m =>
        m.key.toLowerCase().includes(term) ||
        m.text.toLowerCase().includes(term)
    );
  }

  createMacro(): void {
    if (!this.newMacro.key.trim() || !this.newMacro.text.trim()) return;
    this.macros = [...this.macros, { id: this.nextId++, ...this.newMacro }];
    this.clearForm();
  }

  editMacro(macro: Macro): void {
    this.newMacro = { key: macro.key, text: macro.text };
    this.deleteMacro(macro.id);
  }

  deleteMacro(id: number): void {
    this.macros = this.macros.filter(m => m.id !== id);
  }

  clearForm(): void {
    this.newMacro = { key: '', text: '' };
  }
}