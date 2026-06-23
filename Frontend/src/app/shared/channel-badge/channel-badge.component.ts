import { Component, input, computed } from '@angular/core';

type CanalTipo = 'whatsapp' | 'instagram' | 'messenger' | 'otro';

@Component({
  selector: 'app-channel-badge',
  standalone: true,
  template: `
    @switch (tipo()) {
      @case ('whatsapp') {
        <svg viewBox="0 0 32 32" [attr.aria-label]="'WhatsApp'">
          <circle cx="16" cy="16" r="16" fill="#25D366"/>
          <path fill="#fff" d="M16 7.2c-4.9 0-8.8 3.9-8.8 8.8 0 1.6.4 3.1 1.2 4.4L7.2 24.8l4.5-1.2c1.3.7 2.7 1 4.3 1 4.9 0 8.8-3.9 8.8-8.8S20.9 7.2 16 7.2zm0 16c-1.4 0-2.7-.4-3.8-1l-.3-.2-2.7.7.7-2.6-.2-.3c-.7-1.1-1-2.3-1-3.6 0-3.7 3-6.7 6.7-6.7 3.7 0 6.7 3 6.7 6.7s-3 6.7-6.8 6.7zm3.9-5c-.2-.1-1.3-.6-1.5-.7-.2-.1-.3-.1-.5.1-.1.2-.5.7-.6.8-.1.1-.2.2-.4.1-.2-.1-.9-.3-1.7-1-.6-.6-1-1.3-1.2-1.5-.1-.2 0-.3.1-.4l.3-.4c.1-.1.1-.2.2-.4 0-.1 0-.3 0-.4 0-.1-.5-1.1-.6-1.6-.2-.4-.4-.4-.5-.4h-.4c-.1 0-.4.1-.5.3-.2.2-.7.7-.7 1.8s.8 2.1.9 2.2c.1.2 1.6 2.4 3.7 3.3.5.2.9.4 1.2.5.5.2 1 .1 1.3.1.4-.1 1.2-.5 1.4-1 .2-.5.2-.9.1-1z"/>
        </svg>
      }
      @case ('instagram') {
        <svg viewBox="0 0 32 32" [attr.aria-label]="'Instagram'">
          <rect width="32" height="32" rx="9" fill="#E1306C"/>
          <rect x="9" y="9" width="14" height="14" rx="4.5" fill="none" stroke="#fff" stroke-width="2"/>
          <circle cx="16" cy="16" r="3.6" fill="none" stroke="#fff" stroke-width="2"/>
          <circle cx="20.4" cy="11.6" r="1.2" fill="#fff"/>
        </svg>
      }
      @case ('messenger') {
        <svg viewBox="0 0 32 32" [attr.aria-label]="'Messenger'">
          <circle cx="16" cy="16" r="16" fill="#0084FF"/>
          <path fill="#fff" d="M16 7.5c-4.8 0-8.6 3.5-8.6 8 0 2.4 1.1 4.5 2.9 5.9v3l2.7-1.5c.7.2 1.5.3 2.3.3 4.8 0 8.6-3.5 8.6-8s-3.8-8-8.6-8zm.9 10.8l-2.2-2.3-4.3 2.3 4.7-5 2.3 2.3 4.2-2.3-4.7 5z"/>
        </svg>
      }
    }
  `,
  styles: [`
    :host {
      display: inline-block;
      width: 100%;
      height: 100%;
      line-height: 0;
    }
    svg {
      width: 100%;
      height: 100%;
      display: block;
      border-radius: 50%;
    }
  `]
})
export class ChannelBadgeComponent {
  canal = input<string | undefined>('');

  tipo = computed<CanalTipo>(() => {
    const c = (this.canal() ?? '').toLowerCase();
    if (c.includes('whats')) return 'whatsapp';
    if (c.includes('insta')) return 'instagram';
    if (c.includes('messenger') || c.includes('facebook')) return 'messenger';
    return 'otro';
  });
}
