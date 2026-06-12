import { useState } from 'react';
import {
  ActivityIndicator,
  Alert,
  Pressable,
  ScrollView,
  Text,
  TextInput,
  View,
} from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';
import { Trash2 } from 'lucide-react-native';

import {
  type JournalEntry,
  type JournalSeverity,
  useCreateJournalEntry,
  useDeleteJournalEntry,
  useJournal,
} from '@/features/journal/api';

const severityStyles: Record<JournalSeverity, { bg: string; text: string }> = {
  Mild: { bg: 'bg-mint', text: 'text-sage-900' },
  Moderate: { bg: 'bg-peach', text: 'text-sage-900' },
  Severe: { bg: 'bg-coral', text: 'text-cream' },
};

const SeverityChip = ({ severity }: { severity: JournalSeverity }) => {
  const style = severityStyles[severity];
  return (
    <View className={`self-start px-3 py-1 rounded-full ${style.bg}`}>
      <Text
        className={`text-xs uppercase tracking-widest font-display-medium ${style.text}`}
      >
        {severity}
      </Text>
    </View>
  );
};

const TagRow = ({ label, values }: { label: string; values: string[] }) => {
  if (values.length === 0) return null;
  return (
    <View className="flex-row mt-2">
      <Text className="text-sage-700 text-[10px] uppercase tracking-widest w-24 mt-0.5">
        {label}
      </Text>
      <Text className="text-sage-900 text-sm flex-1">{values.join(', ')}</Text>
    </View>
  );
};

const EntryCard = ({ entry }: { entry: JournalEntry }) => {
  const remove = useDeleteJournalEntry();

  const confirmDelete = () =>
    Alert.alert('Delete entry?', 'This cannot be undone.', [
      { text: 'Cancel', style: 'cancel' },
      {
        text: 'Delete',
        style: 'destructive',
        onPress: () => remove.mutate(entry.id),
      },
    ]);

  return (
    <View className="bg-cream rounded-3xl p-5">
      <View className="flex-row items-center justify-between">
        <SeverityChip severity={entry.severity} />
        <Pressable onPress={confirmDelete} className="active:opacity-70 p-1">
          <Trash2 size={18} color="#8FA395" strokeWidth={1.5} />
        </Pressable>
      </View>
      <Text className="text-sage-900 text-base font-display-medium mt-3">
        {entry.summary}
      </Text>
      <Text className="text-sage-700 text-sm mt-1 italic leading-snug">
        &ldquo;{entry.text}&rdquo;
      </Text>
      <TagRow label="Symptoms" values={entry.symptoms} />
      <TagRow label="Triggers" values={entry.triggers} />
      <TagRow label="Treatments" values={entry.treatments} />
      <TagRow label="Areas" values={entry.areas} />
    </View>
  );
};

const EntriesBody = ({
  isLoading,
  isError,
  entries,
  onRetry,
}: {
  isLoading: boolean;
  isError: boolean;
  entries: JournalEntry[];
  onRetry: () => void;
}) => {
  if (isLoading) {
    return (
      <View className="items-center py-8">
        <ActivityIndicator color="#3A453E" />
      </View>
    );
  }
  if (isError) {
    return (
      <Pressable onPress={onRetry} className="items-center py-8 active:opacity-70">
        <Text className="text-sage-800">Couldn&apos;t load entries. Tap to retry.</Text>
      </Pressable>
    );
  }
  if (entries.length === 0) {
    return (
      <Text className="text-sage-700 text-center py-8">
        No entries yet — write your first above.
      </Text>
    );
  }
  return (
    <View className="gap-4">
      {entries.map((e) => (
        <EntryCard key={e.id} entry={e} />
      ))}
    </View>
  );
};

const JournalScreen = () => {
  const [text, setText] = useState('');
  const create = useCreateJournalEntry();
  const { data: entries = [], isLoading, isError, refetch } = useJournal();

  const onSave = () => {
    const trimmed = text.trim();
    if (trimmed.length === 0) return;
    create.mutate(trimmed, {
      onSuccess: () => setText(''),
      onError: () => Alert.alert("Couldn't save", 'Please try again.'),
    });
  };

  return (
    <SafeAreaView edges={['top']} className="flex-1 bg-sage-200">
      <ScrollView
        contentContainerClassName="px-6 pt-8 pb-6"
        keyboardShouldPersistTaps="handled"
      >
        <Text className="text-4xl text-sage-900 font-display">Journal</Text>
        <Text className="text-base text-sage-800 mt-1 mb-4">
          Note how your skin feels — we&apos;ll tag the key details.
        </Text>

        <View className="bg-cream rounded-3xl p-5">
          <TextInput
            value={text}
            onChangeText={setText}
            placeholder="e.g. Elbows itchy today, think it's the new detergent. Used hydrocortisone."
            placeholderTextColor="#8FA395"
            multiline
            editable={!create.isPending}
            className="text-sage-900 min-h-20"
          />
          <Pressable
            onPress={onSave}
            disabled={create.isPending || text.trim().length === 0}
            className="bg-sage-900 rounded-2xl py-3 px-6 mt-3 active:opacity-80 disabled:opacity-40"
          >
            <Text className="text-cream text-center text-base font-display-medium">
              {create.isPending ? 'Reading your entry…' : 'Save entry'}
            </Text>
          </Pressable>
        </View>

        <View className="mt-6">
          <EntriesBody
            isLoading={isLoading}
            isError={isError}
            entries={entries}
            onRetry={refetch}
          />
        </View>
      </ScrollView>
    </SafeAreaView>
  );
};

export default JournalScreen;
