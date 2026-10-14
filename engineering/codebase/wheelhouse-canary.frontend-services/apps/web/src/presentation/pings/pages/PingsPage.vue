<script setup lang="ts">
import { shallowRef } from 'vue';
import { Button } from '@wow-two-beta/ui-vue/presentation/actions';
import { Badge, Card } from '@wow-two-beta/ui-vue/presentation/display';
import { TextInput } from '@wow-two-beta/ui-vue/presentation/forms';
import { usePings } from '../../../application/pings/UsePings';
import { useSystemStatus } from '../../../application/system/UseSystemStatus';

/** Renders the latest pings, the add-ping form, and the running frontend and backend versions. */
defineOptions({ name: 'PingsPage' });

const appVersion = __APP_VERSION__;
const { pings, error, adding, add } = usePings();
const { status } = useSystemStatus();
const note = shallowRef('');

async function submit() {
  await add(note.value);
  note.value = '';
}
</script>

<template>
  <main class="mx-auto min-h-screen max-w-2xl px-6 py-16 font-sans text-foreground">
    <header class="flex flex-wrap items-center gap-3">
      <h1 class="text-3xl font-semibold">WheelhouseCanary</h1>
      <div class="contents" aria-label="App version">
        <Badge>app {{ appVersion }}</Badge>
      </div>
      <div v-if="status" class="contents" aria-label="API version">
        <Badge>api {{ status.version }}</Badge>
      </div>
      <div v-if="status && status.label" class="contents" aria-label="Environment">
        <Badge>{{ status.label }}</Badge>
      </div>
    </header>

    <section class="mt-8" aria-labelledby="pings-heading">
      <Card class="p-6">
        <h2 id="pings-heading" class="sr-only">Pings</h2>
        <form class="flex gap-3" @submit.prevent="submit">
          <TextInput v-model="note" class="flex-1" placeholder="Note (optional)" maxlength="200" aria-label="Note" />
          <Button type="submit" :is-loading="adding">Add ping</Button>
        </form>

        <p v-if="error" class="mt-4 text-destructive" role="alert">Error: {{ error }}</p>
        <p v-else-if="pings === null" class="mt-4" role="status">Loading…</p>
        <p v-else-if="pings.length === 0" class="mt-4 text-muted-foreground" role="status">No pings yet.</p>
        <ul v-else class="mt-4 divide-y divide-border" aria-label="Latest pings">
          <li v-for="ping in pings" :key="ping.id" class="flex items-baseline justify-between gap-4 py-2">
            <span>{{ ping.note }}</span>
            <time class="shrink-0 text-sm text-muted-foreground" :datetime="ping.createdAt.toISOString()">
              {{ ping.createdAt.toLocaleString() }}
            </time>
          </li>
        </ul>
      </Card>
    </section>
  </main>
</template>
