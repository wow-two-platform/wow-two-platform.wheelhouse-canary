import { ApiFailureFactory, createApiClient } from '@wow-two-beta/ui-vue/foundation/http';
import { ResultExtensions } from '@wow-two-beta/ui-vue/foundation/results';
import type { Ping } from '../../domain/pings/Ping';
import type { PingDto } from './PingDto';

const client = createApiClient();

function isPingDto(value: unknown): value is PingDto {
  return (
    value !== null &&
    typeof value === 'object' &&
    'id' in value &&
    typeof value.id === 'string' &&
    'note' in value &&
    typeof value.note === 'string' &&
    'createdAt' in value &&
    typeof value.createdAt === 'string' &&
    !Number.isNaN(Date.parse(value.createdAt))
  );
}

function toPing(dto: PingDto): Ping {
  return { id: dto.id, note: dto.note, createdAt: new Date(dto.createdAt) };
}

/** Provides same-origin ping requests and wire validation. */
export const pingsApi = {
  list(signal: AbortSignal) {
    return client.get<Ping[]>('/api/pings', {
      signal,
      decode: (value) =>
        Array.isArray(value) && value.every(isPingDto)
          ? ResultExtensions.ok(value.map(toPing))
          : ResultExtensions.fail(ApiFailureFactory.create('protocol')),
    });
  },

  add(note: string, signal: AbortSignal) {
    return client.post<Ping>('/api/pings', {
      signal,
      body: { note },
      decode: (value) =>
        isPingDto(value)
          ? ResultExtensions.ok(toPing(value))
          : ResultExtensions.fail(ApiFailureFactory.create('protocol')),
    });
  },
} as const;
