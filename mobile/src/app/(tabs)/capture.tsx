import { Alert, Pressable, Text, View } from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';
import { useRouter } from 'expo-router';
import { Camera } from 'lucide-react-native';
import * as ImagePicker from 'expo-image-picker';
import * as ImageManipulator from 'expo-image-manipulator';

import { useUploadPhoto } from '@/features/photos/api';

const CONTENT_TYPE = 'image/jpeg';
const MAX_DIMENSION = 1568;

const normaliseToJpeg = async (rawUri: string): Promise<string> => {
  const result = await ImageManipulator.manipulateAsync(
    rawUri,
    [{ resize: { width: MAX_DIMENSION } }],
    { compress: 0.85, format: ImageManipulator.SaveFormat.JPEG }
  );
  return result.uri;
};

const CaptureScreen = () => {
  const router = useRouter();
  const uploadPhoto = useUploadPhoto();

  const handlePicked = async (rawUri: string) => {
    try {
      const localUri = await normaliseToJpeg(rawUri);
      await uploadPhoto.mutateAsync({ localUri, contentType: CONTENT_TYPE });
      router.navigate('/photos');
    } catch (err) {
      Alert.alert('Upload failed', err instanceof Error ? err.message : 'Unknown error');
    }
  };

  const pickFromCamera = async () => {
    const perm = await ImagePicker.requestCameraPermissionsAsync();
    if (!perm.granted) {
      Alert.alert('Camera permission needed', 'Enable camera access in Settings.');
      return;
    }
    const result = await ImagePicker.launchCameraAsync({
      mediaTypes: ['images'],
      quality: 0.8,
    });
    if (!result.canceled && result.assets[0]) {
      await handlePicked(result.assets[0].uri);
    }
  };

  const pickFromLibrary = async () => {
    const result = await ImagePicker.launchImageLibraryAsync({
      mediaTypes: ['images'],
      quality: 0.8,
    });
    if (!result.canceled && result.assets[0]) {
      await handlePicked(result.assets[0].uri);
    }
  };

  const busy = uploadPhoto.isPending;

  return (
    <SafeAreaView edges={['top']} className="flex-1 bg-sage-200">
      <View className="flex-1 px-6 pt-12">
        <Text className="text-4xl text-sage-900 font-display">New entry</Text>
        <Text className="text-base text-sage-800 mt-2">
          Take a photo of the area you want to track.
        </Text>

        <View className="flex-1 justify-center gap-6">
          <View className="bg-cream rounded-3xl px-8 py-10 items-center">
            <View className="w-16 h-16 rounded-full bg-sage-100 items-center justify-center mb-4">
              <Camera size={32} color="#3A453E" strokeWidth={1.5} />
            </View>
            <Text className="text-sage-900 text-lg font-display-medium">
              Capture a new photo
            </Text>
            <Text className="text-sage-700 text-center text-sm mt-1 leading-snug">
              Take a clear, well-lit photo of the area you&apos;re tracking. The
              same angle and lighting each time makes comparisons more accurate.
            </Text>
          </View>

          <View className="gap-3">
            <Pressable
              onPress={pickFromCamera}
            disabled={busy}
            className="bg-sage-900 rounded-2xl py-4 px-6 active:opacity-80 disabled:opacity-50"
          >
            <Text className="text-cream text-center text-lg font-display-medium">
              {busy ? 'Uploading…' : 'Take photo'}
            </Text>
          </Pressable>

          <Pressable
            onPress={pickFromLibrary}
            disabled={busy}
            className="border border-sage-900 rounded-2xl py-4 px-6 active:opacity-80 disabled:opacity-50"
          >
            <Text className="text-sage-900 text-center text-lg font-display-medium">
              Choose from library
            </Text>
          </Pressable>
          </View>
        </View>
      </View>
    </SafeAreaView>
  );
};

export default CaptureScreen;
