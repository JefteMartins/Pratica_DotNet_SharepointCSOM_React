import { 
  createLightTheme
} from '@fluentui/react-components';
import type { BrandVariants, Theme } from '@fluentui/react-components';

// Paleta baseada no design "Fidelity" do Stitch
const hotelBrand: BrandVariants = { 
  10: "#000513",
  20: "#001026",
  30: "#001E42", // Primary Navy
  40: "#002D5E",
  50: "#003D7C",
  60: "#004D9B",
  70: "#005EBB",
  80: "#006FDC",
  90: "#0081FF",
  100: "#3399FF",
  110: "#66B2FF",
  120: "#99CCFF",
  130: "#CCE5FF",
  140: "#E5F2FF",
  150: "#F2F9FF",
  160: "#FBFCFF",
};

const lightTheme = createLightTheme(hotelBrand);

export const hotelTheme: Theme = {
  ...lightTheme,
  borderRadiusLarge: '12px',
  colorNeutralBackground1: '#faf9fc', // Grey10 background
};
