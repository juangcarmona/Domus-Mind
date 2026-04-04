import { useSortable } from "@dnd-kit/sortable";
import { CSS } from "@dnd-kit/utilities";
import { ItemRow } from "./ItemRow";
import type { SharedListItemDetail } from "../../../api/types/sharedListTypes";

interface SortableItemRowProps {
  item: SharedListItemDetail;
  listId: string;
  reorderMode?: boolean;
}

export function SortableItemRow({ item, listId, reorderMode = false }: SortableItemRowProps) {
  const {
    attributes,
    listeners,
    setNodeRef,
    transform,
    transition,
    isDragging,
  } = useSortable({ id: item.itemId, disabled: !reorderMode });

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
        reorderMode={reorderMode}
        dragHandleProps={reorderMode ? { ...listeners, ...attributes } : undefined}
      />
    </div>
  );
}
