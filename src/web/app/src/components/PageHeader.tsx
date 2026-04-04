interface PageHeaderProps {
  title: string;
  subtitle?: string;
  actions?: React.ReactNode;
  nav?: React.ReactNode;
  className?: string;
}

/**
 * PageHeader — compact surface header.
 *
 * Replaces ad-hoc .page-header divs with a shared, consistent header primitive.
 * Use inside .l-surface or directly inside .app-main for non-split surfaces.
 *
 * Usage:
 *   <PageHeader
 *     title="Areas"
 *     actions={<button className="btn btn-sm">Add area</button>}
 *   />
 */
export function PageHeader({ title, subtitle, actions, nav, className }: PageHeaderProps) {
  return (
    <div className={`page-header-bar${className ? ` ${className}` : ""}`}>
      {nav && <div className="page-header-bar-nav">{nav}</div>}
      <div className="page-header-bar-main">
        <div className="page-header-bar-titles">
          <h1 className="page-header-bar-title">{title}</h1>
          {subtitle && <p className="page-header-bar-subtitle">{subtitle}</p>}
        </div>
        {actions && <div className="page-header-bar-actions">{actions}</div>}
      </div>
    </div>
  );
}
