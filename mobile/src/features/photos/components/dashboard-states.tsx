import { Pressable, Text, View } from 'react-native';
import { Camera, CloudOff } from 'lucide-react-native';

import { type Photo } from '@/features/photos/api';

const PromptCard = ({ title, body }: { title: string; body: string }) => (
  <View className="bg-cream rounded-3xl p-5">
    <Text className="text-sage-900 text-lg font-display-medium">{title}</Text>
    <Text className="text-sage-700 text-sm mt-1 leading-snug">{body}</Text>
  </View>
);

export const NoComparisonPrompt = ({ photos }: { photos: Photo[] }) =>
  photos.length >= 2 ? (
    <PromptCard
      title="See your progress"
      body="Compare two of your photos from the Compare tab to track how things are changing."
    />
  ) : (
    <PromptCard
      title="One photo so far"
      body="Capture another photo of the same area to start comparing."
    />
  );

export const EmptyState = () => (
  <View className="bg-cream rounded-3xl px-8 py-10 items-center max-w-xs self-center">
    <View className="w-14 h-14 rounded-full bg-sage-100 items-center justify-center mb-4">
      <Camera size={28} color="#3A453E" strokeWidth={1.5} />
    </View>
    <Text className="text-sage-900 text-lg font-display-medium">No entries yet</Text>
    <Text className="text-sage-700 text-center text-sm mt-1">
      Use the Capture tab to start tracking.
    </Text>
  </View>
);

export const ErrorState = ({ onRetry }: { onRetry: () => void }) => (
  <View className="items-center self-center max-w-xs">
    <View className="bg-coral/20 rounded-3xl px-8 py-10 items-center">
      <View className="w-14 h-14 rounded-full bg-sage-100 items-center justify-center mb-4">
        <CloudOff size={28} color="#3A453E" strokeWidth={1.5} />
      </View>
      <Text className="text-sage-900 text-lg font-display-medium text-center">
        Can&apos;t reach the server
      </Text>
      <Text className="text-sage-700 text-center text-sm mt-1">
        Check your connection and try again.
      </Text>
    </View>
    <Pressable
      onPress={onRetry}
      className="mt-4 bg-sage-900 rounded-2xl py-3 px-10 active:opacity-80"
    >
      <Text className="text-cream text-center font-display-medium">Retry</Text>
    </Pressable>
  </View>
);
