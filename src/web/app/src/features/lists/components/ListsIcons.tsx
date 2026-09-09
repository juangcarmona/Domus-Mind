// Local SVG icons for the Lists surface — kept inline to avoid sprite overhead.

export const IconGrid = () => (
  <svg viewBox="0 0 16 16" fill="none" xmlns="http://www.w3.org/2000/svg">
    <rect x="1" y="1" width="5.5" height="5.5" rx="1" fill="currentColor" />
    <rect x="9.5" y="1" width="5.5" height="5.5" rx="1" fill="currentColor" />
    <rect x="1" y="9.5" width="5.5" height="5.5" rx="1" fill="currentColor" />
    <rect x="9.5" y="9.5" width="5.5" height="5.5" rx="1" fill="currentColor" />
  </svg>
);

export const IconList = () => (
  <svg viewBox="0 0 16 16" fill="none" xmlns="http://www.w3.org/2000/svg">
    <rect x="1" y="3" width="14" height="1.5" rx="0.75" fill="currentColor" />
    <rect x="1" y="7.25" width="14" height="1.5" rx="0.75" fill="currentColor" />
    <rect x="1" y="11.5" width="14" height="1.5" rx="0.75" fill="currentColor" />
  </svg>
);

export const IconOptions = () => (
  <svg viewBox="0 0 16 16" fill="none" xmlns="http://www.w3.org/2000/svg">
    <circle cx="3" cy="8" r="1.3" fill="currentColor" />
    <circle cx="8" cy="8" r="1.3" fill="currentColor" />
    <circle cx="13" cy="8" r="1.3" fill="currentColor" />
  </svg>
);

export const IconCalendar = () => (
  <svg viewBox="0 0 16 16" fill="none" xmlns="http://www.w3.org/2000/svg">
    <rect x="1.5" y="3" width="13" height="11" rx="1.5" stroke="currentColor" strokeWidth="1.4" />
    <path d="M1.5 6.5h13" stroke="currentColor" strokeWidth="1.4" />
    <path d="M5 1.5v3M11 1.5v3" stroke="currentColor" strokeWidth="1.4" strokeLinecap="round" />
  </svg>
);

export const IconBell = () => (
  <svg viewBox="0 0 16 16" fill="none" xmlns="http://www.w3.org/2000/svg">
    <path
      d="M8 2a4.5 4.5 0 0 0-4.5 4.5V9.5L2 11h12l-1.5-1.5V6.5A4.5 4.5 0 0 0 8 2z"
      stroke="currentColor"
      strokeWidth="1.4"
      strokeLinejoin="round"
    />
    <path d="M6.5 12.5a1.5 1.5 0 0 0 3 0" stroke="currentColor" strokeWidth="1.4" />
  </svg>
);

export const IconRepeat = () => (
  <svg viewBox="0 0 16 16" fill="none" xmlns="http://www.w3.org/2000/svg">
    <path d="M3 5h8.5a2 2 0 0 1 2 2v1" stroke="currentColor" strokeWidth="1.4" strokeLinecap="round" />
    <path d="M5.5 2.5L3 5l2.5 2.5" stroke="currentColor" strokeWidth="1.4" strokeLinecap="round" strokeLinejoin="round" />
    <path d="M13 11H4.5a2 2 0 0 1-2-2V8" stroke="currentColor" strokeWidth="1.4" strokeLinecap="round" />
    <path d="M10.5 13.5L13 11l-2.5-2.5" stroke="currentColor" strokeWidth="1.4" strokeLinecap="round" strokeLinejoin="round" />
  </svg>
);

export const IconTrash = () => (
  <svg viewBox="0 0 16 16" fill="none" xmlns="http://www.w3.org/2000/svg">
    <path
      d="M2.5 4h11M5.5 4V2.5h5V4M6.5 7v5M9.5 7v5M3.5 4l.8 9.5h7.4L12.5 4"
      stroke="currentColor"
      strokeWidth="1.4"
      strokeLinecap="round"
      strokeLinejoin="round"
    />
  </svg>
);

export const IconChevronDown = () => (
  <svg viewBox="0 0 16 16" fill="none" xmlns="http://www.w3.org/2000/svg">
    <path d="M4 6l4 4 4-4" stroke="currentColor" strokeWidth="1.6" strokeLinecap="round" strokeLinejoin="round" />
  </svg>
);
