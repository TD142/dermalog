import { useState } from 'react';
import { Alert, Image, Pressable, Text, TextInput, View } from 'react-native';
import { useRouter } from 'expo-router';
import { ArrowRight, Camera, RotateCcw, Trash2 } from 'lucide-react-native';

import {
  type Comparison,
  type Photo,
  useDeleteComparison,
  useUpdateComparison,
} from '@/features/photos/api';
import { TrendChip } from '@/features/photos/components/trend-chip';

export type LocalUriResolver = (objectKey: string) => string | undefined;

const Thumb = ({
  photo,
  label,
  getLocalUri,
}: {
  photo: Photo;
  label: string;
  getLocalUri: LocalUriResolver;
}) => {
  const uri = getLocalUri(photo.objectKey) ?? photo.url;
  return (
    <View className="flex-1">
      <Text className="text-sage-700 text-[10px] uppercase tracking-widest mb-1">
        {label}
      </Text>
      <View className="aspect-square rounded-2xl overflow-hidden bg-sage-100 items-center justify-center">
        {uri ? (
          <Image source={{ uri }} className="w-full h-full" />
        ) : (
          <Camera size={24} color="#8FA395" strokeWidth={1.5} />
        )}
      </View>
    </View>
  );
};

export const ComparisonCard = ({
  comparison,
  getLocalUri,
}: {
  comparison: Comparison;
  getLocalUri: LocalUriResolver;
}) => {
  const router = useRouter();
  const update = useUpdateComparison();
  const remove = useDeleteComparison();
  const [label, setLabel] = useState(comparison.label ?? '');

  const commitLabel = () => {
    const trimmed = label.trim();
    if (trimmed === (comparison.label ?? '')) return;
    update.mutate({ id: comparison.id, label: trimmed });
  };

  const confirmDelete = () =>
    Alert.alert('Delete comparison?', 'This cannot be undone.', [
      { text: 'Cancel', style: 'cancel' },
      {
        text: 'Delete',
        style: 'destructive',
        onPress: () => remove.mutate(comparison.id),
      },
    ]);

  return (
    <View className="bg-cream rounded-3xl p-5">
      <TrendChip trend={comparison.severityTrend} />
      <View className="flex-row items-center gap-2 mt-3">
        <Thumb photo={comparison.before} label="Before" getLocalUri={getLocalUri} />
        <ArrowRight size={20} color="#8FA395" strokeWidth={1.5} />
        <Thumb photo={comparison.after} label="After" getLocalUri={getLocalUri} />
      </View>
      <Text className="text-sage-900 text-base mt-4 leading-snug">
        {comparison.overallSummary}
      </Text>
      <TextInput
        value={label}
        onChangeText={setLabel}
        onBlur={commitLabel}
        onSubmitEditing={commitLabel}
        placeholder="Add a label (e.g. Left elbow)"
        placeholderTextColor="#8FA395"
        className="mt-4 text-sage-900"
      />
      <View className="flex-row items-center justify-between mt-3 border-t border-sage-100 pt-3">
        <Pressable
          onPress={() => router.navigate('/compare')}
          className="flex-row items-center active:opacity-70"
        >
          <RotateCcw size={16} color="#3A453E" strokeWidth={1.5} />
          <Text className="text-sage-900 font-display-medium ml-1.5">Re-compare</Text>
        </Pressable>
        <Pressable onPress={confirmDelete} className="active:opacity-70 p-1">
          <Trash2 size={18} color="#8FA395" strokeWidth={1.5} />
        </Pressable>
      </View>
    </View>
  );
};
