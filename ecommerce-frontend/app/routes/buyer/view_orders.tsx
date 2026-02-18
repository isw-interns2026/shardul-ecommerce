/* eslint-disable @typescript-eslint/no-unsafe-assignment */
import apiClient from "~/axios_instance";
import type { BuyerOrderResponseDto } from "~/types/ResponseDto";
import type { Route } from "./+types/view_orders";
import { Card, CardContent } from "~/components/ui/card";
import { Badge } from "~/components/ui/badge";
import { Package, MapPin, Calendar } from "lucide-react";
import { Link } from "react-router";
import type { OrderStatus } from "~/types/ResponseDto";
import { toast } from "sonner";

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
    const response = await apiClient.get(`/buyer/orders`);
    const orders: BuyerOrderResponseDto[] = response.data;
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

export default function OrdersPage({ loaderData }: Route.ComponentProps) {
  const { orders } = loaderData;

  if (!orders.length) {
    return (
      <div className="flex flex-col items-center justify-center py-24 text-center animate-fade-in">
        <div className="p-5 rounded-2xl bg-primary/10 mb-6">
          <Package className="h-10 w-10 text-primary" />
        </div>
        <h2 className="text-2xl font-bold tracking-tight mb-2">
          No orders yet
        </h2>
        <p className="text-muted-foreground max-w-sm">
          Your order history will appear here once you make your first purchase.
        </p>
      </div>
    );
  }

  const sortedOrders = [...orders].sort((a, b) =>
    b.orderId.localeCompare(a.orderId),
  );

  return (
    <div className="mx-auto max-w-3xl p-6 animate-fade-in">
      <div className="flex items-end justify-between mb-6">
        <div>
          <h1 className="text-2xl font-bold tracking-tight">Order History</h1>
          <p className="text-sm text-muted-foreground mt-1">
            {sortedOrders.length} order{sortedOrders.length !== 1 && "s"}
          </p>
        </div>
      </div>

      <div className="grid gap-3">
        {sortedOrders.map((order) => {
          const orderDate = getDateFromUuidV7(order.orderId);

          return (
            <Link
              key={`${order.orderId}-${order.productId}`}
              to={`/buyer/products/${order.productId}`}
              className="block transition-transform active:scale-[0.99]"
            >
              <Card className="overflow-hidden shadow-sm hover:shadow-md hover:border-primary/30 transition-all cursor-pointer">
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
                        <span>•</span>
                        <span className="font-mono">
                          ID: {order.orderId.slice(0, 8)}
                        </span>
                      </div>

                      <h3 className="font-bold text-base leading-tight group-hover:text-primary transition-colors">
                        {order.productName}
                      </h3>

                      <div className="flex items-center gap-3 text-sm">
                        <span className="text-muted-foreground">
                          Qty:{" "}
                          <span className="text-foreground font-medium">
                            {order.productCount}
                          </span>
                        </span>
                        <Badge
                          variant={
                            statusConfig[order.orderStatus]?.variant ??
                            "secondary"
                          }
                          className="h-5 px-1.5 text-[10px] font-bold uppercase"
                        >
                          {statusConfig[order.orderStatus]?.label ??
                            order.orderStatus}
                        </Badge>
                      </div>
                    </div>

                    {/* Right: Price */}
                    <div className="flex flex-row sm:flex-col justify-between sm:justify-start items-center sm:items-end gap-1 border-t sm:border-t-0 pt-3 sm:pt-0">
                      <div className="text-right">
                        <p className="text-[10px] text-muted-foreground uppercase font-bold leading-none">
                          Total
                        </p>
                        <p className="text-xl font-black text-primary">
                          ₹{order.orderValue.toFixed(2)}
                        </p>
                      </div>
                    </div>
                  </div>

                  {/* Compact Footer */}
                  <div className="mt-3 pt-3 border-t flex items-center gap-2 text-xs text-muted-foreground">
                    <MapPin className="h-3 w-3 shrink-0" />
                    <span className="truncate">
                      Shipping to:{" "}
                      <span className="text-foreground">
                        {order.deliveryAddress}
                      </span>
                    </span>
                  </div>
                </CardContent>
              </Card>
            </Link>
          );
        })}
      </div>
    </div>
  );
}
