import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

import {
  CardModule,
  ButtonModule,
  FormModule
} from '@coreui/angular';

interface Tag {
  id: number;
  name: string;
  desc: string;
  color: string;
}

@Component({
  selector: 'app-tags',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    CardModule,
    ButtonModule,
    FormModule
  ],
  templateUrl: './tags.component.html',
  styleUrl: './tags.component.scss'
})
export class TagsComponent {

  tags: Tag[] = [
    { id: 1, name: 'Cliente',        desc: 'Cliente habitual',    color: '#235347' },
    { id: 2, name: 'Nuevo cliente',  desc: 'Primera atención',    color: '#8EB69B' },
    { id: 3, name: 'Caso pendiente', desc: 'Requiere seguimiento', color: '#FFC107' },
  ];

  newTag: Omit<Tag, 'id'> = { name: '', desc: '', color: '#235347' };
  searchTerm = '';
  private nextId = 4;

  get filteredTags(): Tag[] {
    const term = this.searchTerm.toLowerCase();
    if (!term) return this.tags;
    return this.tags.filter(
      t =>
        t.name.toLowerCase().includes(term) ||
        t.desc.toLowerCase().includes(term)
    );
  }

  createTag(): void {
    if (!this.newTag.name.trim()) return;
    this.tags = [...this.tags, { id: this.nextId++, ...this.newTag }];
    this.clearForm();
  }

  editTag(tag: Tag): void {
    this.newTag = { name: tag.name, desc: tag.desc, color: tag.color };
    this.deleteTag(tag.id);
  }

  deleteTag(id: number): void {
    this.tags = this.tags.filter(t => t.id !== id);
  }

  clearForm(): void {
    this.newTag = { name: '', desc: '', color: '#235347' };
  }
}