interface CompactToolbarProps {
  children: React.ReactNode;
  className?: string;
}

/**
 * CompactToolbar — controls strip for filters, view switches, date navigation.
 *
 * Renders a dense horizontal row of controls.
 * Use below PageHeader or inside a surface canvas area.
 *
 * Usage:
 *   <CompactToolbar>
 *     <ViewSwitch ... />
 *     <DateNavigator ... />
 *     <SearchFieldCompact ... />
 *   </CompactToolbar>
 */
export function CompactToolbar({ children, className }: CompactToolbarProps) {
  return (
    <div className={`compact-toolbar${className ? ` ${className}` : ""}`}>
      {children}
    </div>
  );
}
