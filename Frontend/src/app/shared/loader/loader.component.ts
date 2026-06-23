import { Component, inject } from '@angular/core';
import { LoaderService } from '../../services/loader/loader.service';

@Component({
  selector: 'app-loader',
  standalone: true,
  template: `
    @if (loader.isLoading()) {
      <div class="gv-loader-overlay">
        <div class="gv-loader-box">
          <div class="gv-paw">
            <svg viewBox="0 0 24 24" fill="currentColor" width="34" height="34">
              <ellipse cx="12" cy="16.5" rx="4" ry="3.2"/>
              <circle cx="6" cy="11" r="1.9"/>
              <circle cx="18" cy="11" r="1.9"/>
              <circle cx="9" cy="6.5" r="1.9"/>
              <circle cx="15" cy="6.5" r="1.9"/>
            </svg>
          </div>
          <div class="gv-spinner"></div>
          <div class="gv-loader-text">GramVet<span>CRM</span></div>
        </div>
      </div>
    }
  `,
  styles: [`
    .gv-loader-overlay {
      position: fixed;
      inset: 0;
      z-index: 11000;
      display: flex;
      align-items: center;
      justify-content: center;
      background: rgba(13, 26, 21, 0.55);
      backdrop-filter: blur(2px);
    }

    .gv-loader-box {
      position: relative;
      display: flex;
      flex-direction: column;
      align-items: center;
      gap: 14px;
      padding: 30px 38px;
      background: #ffffff;
      border-radius: 18px;
      box-shadow: 0 12px 40px rgba(0, 0, 0, 0.25);
    }

    .gv-spinner {
      width: 58px;
      height: 58px;
      border-radius: 50%;
      border: 5px solid rgba(35, 83, 71, 0.15);
      border-top-color: #235347;
      animation: gv-spin 0.8s linear infinite;
    }

    /* Patita en el centro del spinner */
    .gv-paw {
      position: absolute;
      top: 42px;
      color: #235347;
      animation: gv-pulse 1.2s ease-in-out infinite;
    }

    .gv-loader-text {
      font-size: 15px;
      font-weight: 700;
      letter-spacing: 0.5px;
      color: #235347;
    }

    .gv-loader-text span {
      color: #8aaa98;
      margin-left: 3px;
    }

    @keyframes gv-spin {
      to { transform: rotate(360deg); }
    }

    @keyframes gv-pulse {
      0%, 100% { transform: scale(1); opacity: 0.85; }
      50% { transform: scale(1.12); opacity: 1; }
    }
  `]
})
export class LoaderComponent {
  loader = inject(LoaderService);
}
