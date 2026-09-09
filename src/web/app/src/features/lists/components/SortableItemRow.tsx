import { useSortable } from "@dnd-kit/sortable";
import { CSS } from "@dnd-kit/utilities";
import { ItemRow } from "./ItemRow";
import type { SharedListItemDetail } from "../../../api/types/listTypes";

interface SortableItemRowProps {
  item: SharedListItemDetail;
  listId: string;
  selectedItemId?: string | null;
  onSelect?: (item: SharedListItemDetail, listId: string) => void;
  onToggle?: (item: SharedListItemDetail) => void;
}

export function SortableItemRow({
  item,
  listId,
  selectedItemId,
  onSelect,
  onToggle,
}: SortableItemRowProps) {
  const {
    attributes,
    listeners,
    setNodeRef,
    transform,
    transition,
    isDragging,
  } = useSortable({ id: item.itemId });

  return (
    <div
      ref={setNodeRef}
      style={{
        transform: CSS.Transform.toString(transform),
        transition,
        opacity: isDragging ? 0.35 : undefined,
        zIndex: isDragging ? 10 : undefined,
        position: "relative",
      }}
    >
      <ItemRow
        item={item}
        listId={listId}
        selectedItemId={selectedItemId}
        onSelect={onSelect}
        onToggle={onToggle}
        dragHandleProps={{ ...listeners, ...attributes }}
      />
    </div>
  );
}
