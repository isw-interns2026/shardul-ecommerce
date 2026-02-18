/* eslint-disable @typescript-eslint/no-unsafe-assignment */
import { useState } from "react";
import apiClient from "~/axios_instance";
import type { SellerOrderResponseDto, OrderStatus } from "~/types/ResponseDto";
import type { Route } from "./+types/seller_orders";
import { Card, CardContent } from "~/components/ui/card";
import { Badge } from "~/components/ui/badge";
import { Button } from "~/components/ui/button";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
  DialogTrigger,
  DialogClose,
} from "~/components/ui/dialog";
import { Package, MapPin, Calendar, Truck, Loader2 } from "lucide-react";
import { toast } from "sonner";
import { useRevalidator } from "react-router";

const statusConfig: Record<
  OrderStatus,
  {
    label: string;
    variant: "default" | "secondary" | "destructive" | "outline";
  }
> = {
  AwaitingPayment: { label: "Awaiting Payment", variant: "outline" },
  InTransit: { label: "In Transit", variant: "secondary" },
  Delivered: { label: "Delivered", variant: "default" },
  Cancelled: { label: "Cancelled", variant: "destructive" },
};

export async function clientLoader() {
  try {
    const response = await apiClient.get("/seller/orders");
    const orders: SellerOrderResponseDto[] = response.data;
    return { orders };
  } catch {
    toast.error("Failed to load orders. Please refresh the page.");
    return { orders: [] };
  }
}

function getDateFromUuidV7(uuid: string) {
  try {
    const hexTimestamp = uuid.replace(/-/g, "").substring(0, 12);
    const timestampMs = parseInt(hexTimestamp, 16);
    return new Date(timestampMs);
  } catch {
    return new Date();
  }
}

export default function SellerOrdersPage({ loaderData }: Route.ComponentProps) {
  const { orders } = loaderData;

  if (!orders.length) {
    return (
      <div className="mx-auto max-w-3xl p-6">
        <h1 className="mb-6 text-3xl font-bold tracking-tight">Orders</h1>
        <div className="flex flex-col items-center justify-center p-12 text-muted-foreground">
          <Package className="h-12 w-12 mb-4 opacity-20" />
          <p className="text-lg font-medium">No orders yet</p>
          <p className="text-sm mt-1">
            Orders for your products will appear here.
          </p>
        </div>
      </div>
    );
  }

  const sortedOrders = [...orders].sort((a, b) =>
    b.orderId.localeCompare(a.orderId),
  );

  return (
    <div className="mx-auto max-w-3xl p-6">
      <h1 className="mb-6 text-3xl font-bold tracking-tight">Orders</h1>

      <div className="grid gap-3">
        {sortedOrders.map((order) => (
          <OrderCard
            key={`${order.orderId}-${order.productId}`}
            order={order}
          />
        ))}
      </div>
    </div>
  );
}

function OrderCard({ order }: { order: SellerOrderResponseDto }) {
  const orderDate = getDateFromUuidV7(order.orderId);
  const revalidator = useRevalidator();
  const [isMarking, setIsMarking] = useState(false);
  const [dialogOpen, setDialogOpen] = useState(false);

  const handleMarkDelivered = async () => {
    setIsMarking(true);
    try {
      await apiClient.patch(`/seller/orders/${order.orderId}`, {
        status: "Delivered",
      });
      toast.success("Order marked as delivered!");
      setDialogOpen(false);
      revalidator.revalidate();
    } catch {
      toast.error("Failed to update order status.");
    } finally {
      setIsMarking(false);
    }
  };

  return (
    <Card className="overflow-hidden shadow-sm hover:shadow-md hover:border-primary/30 transition-all">
      <CardContent className="p-4">
        <div className="flex flex-col sm:flex-row justify-between gap-4">
          {/* Left: Product & Meta */}
          <div className="space-y-1 flex-1">
            <div className="flex items-center gap-2 text-[10px] font-medium text-muted-foreground uppercase tracking-wider">
              <Calendar className="h-3 w-3" />
              {orderDate.toLocaleDateString("en-IN", {
                day: "2-digit",
                month: "short",
                year: "numeric",
                hour: "2-digit",
                minute: "2-digit",
              })}
              <span>&#183;</span>
              <span className="font-mono">ID: {order.orderId.slice(0, 8)}</span>
            </div>

            <h3 className="font-bold text-base leading-tight">
              {order.productName}
            </h3>

            <div className="flex items-center gap-3 text-sm flex-wrap">
              <span className="text-muted-foreground">
                SKU:{" "}
                <code className="text-xs bg-muted px-1 py-0.5 rounded">
                  {order.productSku}
                </code>
              </span>
              <span className="text-muted-foreground">
                Qty:{" "}
                <span className="text-foreground font-medium">
                  {order.productCount}
                </span>
              </span>
              <Badge
                variant={
                  statusConfig[order.orderStatus]?.variant ?? "secondary"
                }
                className="h-5 px-1.5 text-[10px] font-bold uppercase"
              >
                {statusConfig[order.orderStatus]?.label ?? order.orderStatus}
              </Badge>
            </div>
          </div>

          {/* Right: Price + Action */}
          <div className="flex flex-row sm:flex-col justify-between sm:justify-start items-center sm:items-end gap-2 border-t sm:border-t-0 pt-3 sm:pt-0">
            <div className="text-right">
              <p className="text-[10px] text-muted-foreground uppercase font-bold leading-none">
                Total
              </p>
              <p className="text-xl font-black text-primary">
                ₹{order.orderValue.toFixed(2)}
              </p>
            </div>

            {order.orderStatus === "InTransit" && (
              <Dialog open={dialogOpen} onOpenChange={setDialogOpen}>
                <DialogTrigger asChild>
                  <Button size="sm" variant="outline" className="text-xs">
                    <Truck className="mr-1 h-3.5 w-3.5" />
                    Mark Delivered
                  </Button>
                </DialogTrigger>
                <DialogContent>
                  <DialogHeader>
                    <DialogTitle>Confirm Delivery</DialogTitle>
                    <DialogDescription>
                      Mark order{" "}
                      <span className="font-mono font-bold">
                        {order.orderId.slice(0, 8)}
                      </span>{" "}
                      for <span className="font-bold">{order.productName}</span>{" "}
                      as delivered? This action cannot be undone.
                    </DialogDescription>
                  </DialogHeader>
                  <DialogFooter className="gap-2">
                    <DialogClose asChild>
                      <Button variant="outline" disabled={isMarking}>
                        Cancel
                      </Button>
                    </DialogClose>
                    <Button onClick={handleMarkDelivered} disabled={isMarking}>
                      {isMarking ? (
                        <>
                          <Loader2 className="mr-2 h-4 w-4 animate-spin" />
                          Updating...
                        </>
                      ) : (
                        "Confirm Delivered"
                      )}
                    </Button>
                  </DialogFooter>
                </DialogContent>
              </Dialog>
            )}
          </div>
        </div>

        {/* Footer */}
        <div className="mt-3 pt-3 border-t flex items-center gap-2 text-xs text-muted-foreground">
          <MapPin className="h-3 w-3 shrink-0" />
          <span className="truncate">
            Delivery to:{" "}
            <span className="text-foreground">{order.deliveryAddress}</span>
          </span>
        </div>
      </CardContent>
    </Card>
  );
}
