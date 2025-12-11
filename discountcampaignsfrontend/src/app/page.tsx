"use client";
import {
  Autocomplete,
  Box,
  Button,
  Card,
  IconButton,
  TextField,
  Typography,
} from "@mui/material";
import Image from "next/image";
import { useEffect, useRef, useState } from "react";
import {
  calculateTotalSum,
  getProducts,
  Product,
  DiscountResponse,
} from "../../services/postService";
import { Add, Remove } from "@mui/icons-material";
import AddIcon from "@mui/icons-material/Add";
import RemoveIcon from "@mui/icons-material/Remove";

interface Campaign {
  campaignName: string;
  category: string;
}

interface SelectedProduct {
  product: Product;
  quantity: number;
}

interface CampaignSelection {
  selectedCampaignCoupon: Campaign | null;
  discountCoupon: number | string;
  selectedCampaignOnTop: Campaign | null;
  discountOnTopClassify: string;
  discountOnTopValue: number | string;
  selectedCampaignSeasonal: Campaign | null;
  discountSeasonalClassify: number | string;
  discountSeasonalValue: number | string;
}

export interface DiscountRequest {
  selectedProduct: SelectedProduct[];
  campaignsSelected: CampaignSelection;
  userInput: Record<string, any>;
}

export default function Home() {
  const campaignList: Campaign[] = [
    {
      campaignName: "Fix amount",
      category: "Coupon",
    },
    {
      campaignName: "Percentage discount",
      category: "Coupon",
    },
    {
      campaignName: "Percentage discount by item category",
      category: "On Top",
    },
    {
      campaignName: "Discount by points",
      category: "On Top",
    },
    {
      campaignName: "Special campaigns",
      category: "Seasonal",
    },
  ];
  const [product, setProducts] = useState<Product[]>([]);
  const [loading, setLoading] = useState(true);
  const [selectedProducts, setSelectedProducts] = useState<SelectedProduct[]>(
    []
  );
  const [selectedCampaignCoupon, setSelectedCampaignCoupon] =
    useState<Campaign | null>(null);
  const [discountCoupon, setDiscountCoupon] = useState<string>("");
  const [selectedCampaignOnTop, setSelectedCampaignOnTop] =
    useState<Campaign | null>(null);
  const [discountOnTopClassify, setDiscountOnTopClassify] =
    useState<string>("");
  const [discountOnTopValue, setDiscountOnTopValue] = useState<string>("");
  const [selectedCampaignSeasonal, setSelectedCampaignSeasonal] =
    useState<Campaign | null>(null);
  const [discountSeasonalClassify, setDiscountSeasonalClassify] =
    useState<string>("");
  const [discountSeasonalValue, setDiscountSeasonalValue] =
    useState<string>("");
  const [inputValue, setInputValue] = useState("");
  const inputRef = useRef<HTMLInputElement | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [sum, setSum] = useState<string>();
  const [totalPrice, setTotalPrice] = useState<string>();

  const handleSetDiscountCoupon = (e: React.ChangeEvent<HTMLInputElement>) => {
    const value = e.target.value;
    const numericValue = Number(value);

    if (value === "") {
      setDiscountCoupon("");
    } else if (
      selectedCampaignCoupon?.campaignName == "Percentage discount" &&
      numericValue >= 0 &&
      numericValue <= 100
    ) {
      setDiscountCoupon(numericValue.toString());
    } else if (
      selectedCampaignCoupon?.campaignName == "Fix amount" &&
      numericValue >= 0
    ) {
      setDiscountCoupon(numericValue.toString());
    }
  };

  const handleAddProduct = (_: any, product: Product | null) => {
    if (!product) return;

    const existing = selectedProducts.find(
      (p) => p.product.name === product.name
    );
    if (existing) {
      setSelectedProducts((prev) =>
        prev.map((p) =>
          p.product.name === product.name
            ? { ...p, quantity: p.quantity + 1 }
            : p
        )
      );
    } else {
      setSelectedProducts((prev) => [...prev, { product, quantity: 1 }]);
    }
  };

  const increaseQty = (name: string) => {
    setSelectedProducts((prev) =>
      prev.map((p) =>
        p.product.name === name ? { ...p, quantity: p.quantity + 1 } : p
      )
    );
  };

  const decreaseQty = (name: string) => {
    setSelectedProducts((prev) =>
      prev
        .map((p) =>
          p.product.name === name ? { ...p, quantity: p.quantity - 1 } : p
        )
        .filter((p) => p.quantity > 0)
    );
  };

  const handleCalculateTotal = async () => {
    if (selectedProducts.length === 0) {
      alert("Please select at least one product");
      return;
    }

    if (selectedCampaignCoupon && discountCoupon === "") {
      alert("Please fill discount for campaign coupon");
      return;
    }

    if (selectedCampaignOnTop) {
      if (
        selectedCampaignOnTop.campaignName ===
          "Percentage discount by item category" &&
        (discountOnTopClassify === "" || discountOnTopValue === "")
      ) {
        alert("Please choose category and fill amount for campaign on top");
        return;
      }

      if (
        selectedCampaignOnTop.campaignName === "Discount by points" &&
        discountOnTopValue === ""
      ) {
        alert("Please fill point for campaign on top");
        return;
      }
    }

    if (selectedCampaignSeasonal) {
      if (
        selectedCampaignSeasonal.campaignName === "Special campaigns" &&
        (discountSeasonalClassify === "" || discountSeasonalValue === "")
      ) {
        alert("Please fill 2 input in campaign seasonal");
        return;
      }

      if (
        selectedCampaignSeasonal.campaignName !== "Special campaigns" &&
        discountSeasonalValue === ""
      ) {
        alert("Please fill amount in campaign seasonal");
        return;
      }
    }
    // สร้าง userInput object ตามเงื่อนไขที่ backend ต้องการ
    const userInput: Record<string, any> = {};
    if (selectedCampaignOnTop?.campaignName === "Discount by points") {
      userInput.points = Number(discountOnTopValue) || 0;
    }
    if (selectedCampaignCoupon?.campaignName === "Percentage discount") {
      userInput.percentcoupon = Number(discountCoupon) || 0;
    }
    if (selectedCampaignCoupon?.campaignName === "Fix amount") {
      userInput.amount = Number(discountCoupon) || 0;
    }
    if (
      selectedCampaignOnTop?.campaignName ===
      "Percentage discount by item category"
    ) {
      userInput.category = discountOnTopClassify;
      userInput.percent = Number(discountOnTopValue) || 0;
    }
    if (selectedCampaignSeasonal?.campaignName === "Special campaigns") {
      userInput.step = Number(discountSeasonalClassify) || 0;
      userInput.discount = Number(discountSeasonalValue) || 0;
    }
    // สามารถเพิ่ม mapping เงื่อนไขอื่น ๆ ตาม backend ได้ที่นี่

    const campaignSelection: CampaignSelection = {
      selectedCampaignCoupon,
      discountCoupon,
      selectedCampaignOnTop,
      discountOnTopClassify,
      discountOnTopValue,
      selectedCampaignSeasonal,
      discountSeasonalClassify,
      discountSeasonalValue,
    };

    const discountRequest: DiscountRequest = {
      campaignsSelected: campaignSelection,
      selectedProduct: selectedProducts,
      userInput,
    };
    const result = await calculateTotalSum(discountRequest);
    if (result && typeof result === "object" && "final" in result) {
      const roundedUp = Math.ceil(result.final * 100) / 100;
      setSum(
        new Intl.NumberFormat("th-TH", {
          minimumFractionDigits: 2,
          maximumFractionDigits: 2,
        }).format(roundedUp)
      );
    } else {
      setSum("0.00");
    }
  };

  useEffect(() => {
    setDiscountCoupon("");
  }, [selectedCampaignCoupon]);
  useEffect(() => {
    setDiscountOnTopClassify("");
    setDiscountOnTopValue("");
  }, [selectedCampaignOnTop]);
  useEffect(() => {
    setDiscountSeasonalClassify("");
    setDiscountSeasonalValue("");
  }, [selectedCampaignSeasonal]);

  useEffect(() => {
    getProducts()
      .then(setProducts)
      .catch((err) => console.error("โหลดโพสต์ไม่สำเร็จ:", err.message))
      .finally(() => setLoading(false));
  }, []);

  return (
    // <div className="flex min-h-screen p-10">
    <div className="flex flex-col lg:flex-row min-h-screen p-4 lg:p-10 gap-4">
      {/* <div className="w-2/5 p-4"> */}
      <div className="w-full lg:w-2/5 p-0 lg:p-4">
        <Card
          sx={{
            height: "100%",
            display: "flex",
            flexDirection: "column",
            alignItems: "center",
          }}
        >
          <Autocomplete
            disablePortal
            options={product.filter((item) =>
              selectedProducts.every((st) => st.product.name !== item.name)
            )}
            getOptionLabel={(option) => option.name}
            value={null}
            inputValue={inputValue}
            onInputChange={(_, newInputValue) => setInputValue(newInputValue)}
            onChange={(e, v) => {
              handleAddProduct(e, v);
              setInputValue("");
              const input = e?.target as HTMLInputElement;
              input?.blur();
              inputRef.current?.blur();
            }}
            sx={{ width: "70%", mt: 5 }}
            renderInput={(params) => (
              <TextField {...params} inputRef={inputRef} label="Products" />
            )}
          />
          <Box mt={2} width={"70%"}>
            {selectedProducts.map(({ product, quantity }) => (
              <Box
                key={product.name}
                display="flex"
                alignItems="center"
                justifyContent="space-between"
                px={2}
                py={1}
                mb={2}
              >
                <Typography>{product.name}</Typography>
                <Box display="flex" alignItems="center" gap={1}>
                  <TextField
                    key={product.name}
                    label="Unit Price (Bath)"
                    value={product.price}
                    InputProps={{
                      readOnly: true,
                    }}
                    size="small"
                  />
                  <IconButton
                    onClick={() => decreaseQty(product.name)}
                    color="error"
                  >
                    <RemoveIcon />
                  </IconButton>
                  <Typography>{quantity}</Typography>
                  <IconButton
                    onClick={() => increaseQty(product.name)}
                    color="primary"
                  >
                    <AddIcon />
                  </IconButton>
                </Box>
              </Box>
            ))}
          </Box>
        </Card>
      </div>
      {/* <div className="w-3/5 p-4 flex flex-col gap-4"> */}
      <div className="w-full lg:w-3/5 p-0 lg:p-4 flex flex-col gap-4">
        <Card
          sx={{
            height: "80%",
            display: "flex",
            flexDirection: "column",
            alignItems: "center",
            justifyContent: "center",
            gap: 2,
          }}
          className="pb-4 pt-4 lg:pb-0 lg:pt-0"
        >
          <Autocomplete
            disablePortal
            options={campaignList.filter((c) => c.category === "Coupon")}
            getOptionLabel={(option) => option.campaignName}
            onChange={(evnet, c) => setSelectedCampaignCoupon(c)}
            sx={{ width: "70%" }}
            renderInput={(params) => (
              <TextField {...params} label="Campaigns : Counpon" />
            )}
          />
          <TextField
            label={
              selectedCampaignCoupon?.campaignName == "Fix amount"
                ? "Amount (Bath)"
                : selectedCampaignCoupon?.campaignName == "Percentage discount"
                ? "Amount (%)"
                : "Amount"
            }
            type="number"
            value={discountCoupon}
            onChange={handleSetDiscountCoupon}
            onKeyDown={(e) => {
              if (
                ["e", "E", "+", "-"].includes(e.key) ||
                (e.ctrlKey && e.key === "v")
              ) {
                e.preventDefault();
              }
            }}
            inputProps={{
              min: 0,
              max:
                selectedCampaignCoupon?.campaignName == "Percentage discount"
                  ? 100
                  : undefined,
              step: 1,
            }}
            variant="outlined"
            sx={{ width: "70%" }}
          />
          <Autocomplete
            disablePortal
            key={selectedCampaignOnTop?.campaignName || "noneOnTop"}
            options={campaignList.filter((c) => c.category === "On Top")}
            getOptionLabel={(option) => option.campaignName}
            value={selectedCampaignOnTop}
            onChange={(evnet, c) => setSelectedCampaignOnTop(c)}
            sx={{ width: "70%" }}
            renderInput={(params) => (
              <TextField {...params} label="Campaigns : On Top" />
            )}
          />

          {selectedCampaignOnTop?.campaignName ==
          "Percentage discount by item category" ? (
            <div className="w-[70%] flex">
              <Autocomplete
                options={Array.from(new Set(product.map((p) => p.category)))}
                value={discountOnTopClassify || ""}
                onChange={(_, val) => setDiscountOnTopClassify(val || "")}
                sx={{ width: "50%" }}
                renderInput={(params) => (
                  <TextField {...params} label="Category" />
                )}
              />
              <TextField
                label="Amount (%)"
                type="number"
                value={discountOnTopValue}
                onChange={(e) => {
                  const val = Number(e.target.value);
                  if (val >= 0 && val <= 100) {
                    setDiscountOnTopValue(val.toString());
                  } else if (e.target.value === "") {
                    setDiscountOnTopValue("");
                  }
                }}
                onKeyDown={(e) => {
                  if (
                    ["e", "E", "+", "-"].includes(e.key) ||
                    (e.ctrlKey && e.key === "v")
                  ) {
                    e.preventDefault();
                  }
                }}
                inputProps={{ min: 0, max: 100, step: 1 }}
                variant="outlined"
                sx={{ width: "50%" }}
              />
            </div>
          ) : (
            <TextField
              label={
                selectedCampaignCoupon == null ||
                selectedCampaignOnTop?.campaignName !== "Discount by points"
                  ? "Amount"
                  : "Point"
              }
              type="number"
              value={discountOnTopValue}
              onChange={(e) => {
                const val = Number(e.target.value);
                if (val >= 0) {
                  setDiscountOnTopValue(val.toString());
                } else if (e.target.value === "") {
                  setDiscountOnTopValue("");
                }
              }}
              onKeyDown={(e) => {
                if (
                  ["e", "E", "+", "-"].includes(e.key) ||
                  (e.ctrlKey && e.key === "v")
                ) {
                  e.preventDefault();
                }
              }}
              inputProps={{
                min: 0,
                step: 1,
              }}
              variant="outlined"
              sx={{ width: "70%" }}
            />
          )}

          <Autocomplete
            disablePortal
            key={selectedCampaignSeasonal?.campaignName || "noneSeasonal"}
            options={campaignList.filter((c) => c.category === "Seasonal")}
            getOptionLabel={(option) => option.campaignName}
            value={selectedCampaignSeasonal}
            onChange={(evnet, c) => setSelectedCampaignSeasonal(c)}
            sx={{ width: "70%" }}
            renderInput={(params) => (
              <TextField {...params} label="Campaigns : Seasonal" />
            )}
          />
          {selectedCampaignSeasonal?.campaignName == "Special campaigns" ? (
            <div className="w-[70%] flex">
              <TextField
                label="Every (Bath)"
                type="number"
                value={discountSeasonalClassify}
                onChange={(e) => {
                  const val = Number(e.target.value);
                  if (val >= 0) {
                    setDiscountSeasonalClassify(val.toString());
                  } else if (e.target.value === "") {
                    setDiscountSeasonalClassify("");
                  }
                }}
                onKeyDown={(e) => {
                  if (
                    ["e", "E", "+", "-"].includes(e.key) ||
                    (e.ctrlKey && e.key === "v")
                  ) {
                    e.preventDefault();
                  }
                }}
                inputProps={{ min: 0, step: 1 }}
                variant="outlined"
                sx={{ width: "50%" }}
              />
              <TextField
                label="Discount (Bath)"
                type="number"
                value={discountSeasonalValue}
                onChange={(e) => {
                  const val = Number(e.target.value);
                  if (val >= 0) {
                    setDiscountSeasonalValue(val.toString());
                  } else if (e.target.value === "") {
                    setDiscountSeasonalValue("");
                  }
                }}
                onKeyDown={(e) => {
                  if (
                    ["e", "E", "+", "-"].includes(e.key) ||
                    (e.ctrlKey && e.key === "v")
                  ) {
                    e.preventDefault();
                  }
                }}
                inputProps={{ min: 0, step: 1 }}
                variant="outlined"
                sx={{ width: "50%" }}
              />
            </div>
          ) : (
            <TextField
              label={"Amount"}
              type="number"
              value={discountSeasonalValue}
              onChange={(e) => {
                const val = Number(e.target.value);
                if (val >= 0) {
                  setDiscountSeasonalValue(val.toString());
                } else if (e.target.value === "") {
                  setDiscountSeasonalValue("");
                }
              }}
              onKeyDown={(e) => {
                if (
                  ["e", "E", "+", "-"].includes(e.key) ||
                  (e.ctrlKey && e.key === "v")
                ) {
                  e.preventDefault();
                }
              }}
              inputProps={{ min: 0, step: 1 }}
              variant="outlined"
              sx={{ width: "70%" }}
            />
          )}
        </Card>
        <Card
          sx={{
            height: "20%",
            display: "flex",
            flexDirection: "column",
            alignItems: "center",
            justifyContent: "center",
            gap: 2,
          }}
          className="pb-4 pt-4 lg:pb-0 lg:pt-0"
        >
          <div className="flex justify-between w-[90%] gap-2">
            <TextField
              key={selectedProducts.reduce(
                (sum, item) => sum + item.product.price * item.quantity,
                0
              )}
              label="Total Price (Bath)"
              value={
                selectedProducts.reduce(
                  (sum, item) => sum + item.product.price * item.quantity,
                  0
                ) == 0
                  ? ""
                  : new Intl.NumberFormat("th-TH", {
                      minimumFractionDigits: 2,
                      maximumFractionDigits: 2,
                    }).format(
                      selectedProducts.reduce(
                        (sum, item) => sum + item.product.price * item.quantity,
                        0
                      )
                    )
              }
              InputProps={{
                readOnly: true,
              }}
              variant="outlined"
              sx={{ width: "30%" }}
            />
            <TextField
              key={sum}
              label="Total Price Discounted (Bath)"
              value={sum}
              InputProps={{
                readOnly: true,
              }}
              variant="outlined"
              sx={{ width: "30%" }}
            />
            <Button variant="contained" onClick={handleCalculateTotal}>
              SUM TOTAL PRICE DISCOUNTED
            </Button>
          </div>
        </Card>
      </div>
    </div>
  );
}
