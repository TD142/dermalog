import { Tabs } from 'expo-router';
import { Camera, GitCompare, House, Images } from 'lucide-react-native';
import { useSafeAreaInsets } from 'react-native-safe-area-context';

const homeIcon = ({ color }: { color: string }) => (
  <House color={color} size={28} strokeWidth={1.5} />
);

const photosIcon = ({ color }: { color: string }) => (
  <Images color={color} size={28} strokeWidth={1.5} />
);

const captureIcon = ({ color }: { color: string }) => (
  <Camera color={color} size={28} strokeWidth={1.5} />
);

const compareIcon = ({ color }: { color: string }) => (
  <GitCompare color={color} size={28} strokeWidth={1.5} />
);

const TabsLayout = () => {
  const insets = useSafeAreaInsets();

  return (
    <Tabs
      screenOptions={{
        headerShown: false,
        tabBarActiveTintColor: '#2D332C',
        tabBarInactiveTintColor: '#8FA395',
        tabBarStyle: {
          backgroundColor: '#F5F1EA',
          borderTopColor: '#E3EAE0',
          height: 64 + insets.bottom,
          paddingTop: 12,
          paddingBottom: insets.bottom,
        },
        tabBarLabelStyle: { fontSize: 11 },
      }}
    >
      <Tabs.Screen name="index" options={{ title: 'Home', tabBarIcon: homeIcon }} />
      <Tabs.Screen name="photos" options={{ title: 'Photos', tabBarIcon: photosIcon }} />
      <Tabs.Screen name="capture" options={{ title: 'Capture', tabBarIcon: captureIcon }} />
      <Tabs.Screen name="compare" options={{ title: 'Compare', tabBarIcon: compareIcon }} />
    </Tabs>
  );
};

export default TabsLayout;
