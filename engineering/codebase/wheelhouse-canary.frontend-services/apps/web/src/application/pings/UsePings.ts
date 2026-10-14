import { onMounted, onScopeDispose, shallowRef } from 'vue';
import type { ApiFailure } from '@wow-two-beta/ui-vue/foundation/http';
import type { Ping } from '../../domain/pings/Ping';
import { pingsApi } from '../../integration/pings';

function describeFailure(failure: ApiFailure): string {
  return failure.status > 0 ? `API returned ${failure.status}` : failure.message;
}

/** Loads the latest pings for this component's lifetime and records new ones. */
export function usePings() {
  const controller = new AbortController();
  const pings = shallowRef<Ping[] | null>(null);
  const error = shallowRef<string | null>(null);
  const adding = shallowRef(false);

  async function load() {
    const result = await pingsApi.list(controller.signal);
    if (controller.signal.aborted) return;
    if (result.ok) {
      pings.value = result.value;
      error.value = null;
    } else if (result.failure.code !== 'cancelled') {
      error.value = describeFailure(result.failure);
    }
  }

  /** Records a ping with the given note, then reloads the list. */
  async function add(note: string) {
    adding.value = true;
    const result = await pingsApi.add(note.trim(), controller.signal);
    if (controller.signal.aborted) return;
    adding.value = false;
    if (result.ok) await load();
    else if (result.failure.code !== 'cancelled') error.value = describeFailure(result.failure);
  }

  onMounted(load);
  onScopeDispose(() => controller.abort());

  return { pings, error, adding, add };
}
