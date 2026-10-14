import { flushPromises, mount } from '@vue/test-utils';
import { afterEach, describe, expect, it, vi } from 'vitest';
import PingsPage from '../src/presentation/pings/pages/PingsPage.vue';

const wrappers: ReturnType<typeof mount>[] = [];

afterEach(() => {
  for (const wrapper of wrappers.splice(0)) wrapper.unmount();
  vi.unstubAllGlobals();
});

function render() {
  const wrapper = mount(PingsPage);
  wrappers.push(wrapper);
  return wrapper;
}

const status = { data: { service: 'wheelhouse-canary', status: 'ok', version: '0.1.4', label: 'dev' } };
const first = { id: '0198a000-0000-7000-8000-000000000001', note: 'first', createdAt: '2026-10-14T10:00:00Z' };
const second = { id: '0198a000-0000-7000-8000-000000000002', note: 'second', createdAt: '2026-10-14T11:00:00Z' };

/** Stubs fetch: the status endpoint, the pings list and the pings insert. */
function stubApi(pings: unknown[] | Response, onPost?: (body: unknown) => void) {
  const fetch = vi.fn<typeof globalThis.fetch>(async (input, init) => {
    const url = String(input);
    if (url === '/api/system/status') return Response.json(status);
    if (init?.method === 'POST') {
      onPost?.(JSON.parse(String(init.body)));
      return Response.json({ data: second }, { status: 201 });
    }
    return pings instanceof Response ? pings : Response.json({ data: pings });
  });
  vi.stubGlobal('fetch', fetch);
  return fetch;
}

describe('pings page', () => {
  it('shows loading, then the latest pings and the running versions', async () => {
    stubApi([second, first]);
    const wrapper = render();
    expect(wrapper.get('[role="status"]').text()).toBe('Loading…');
    await flushPromises();
    expect(wrapper.findAll('li').map((item) => item.get('span').text())).toEqual(['second', 'first']);
    expect(wrapper.get('[aria-label="App version"]').text()).toBe('app test');
    expect(wrapper.get('[aria-label="API version"]').text()).toBe('api 0.1.4');
    expect(wrapper.get('[aria-label="Environment"]').text()).toBe('dev');
  });

  it('says so when there are no pings', async () => {
    stubApi([]);
    const wrapper = render();
    await flushPromises();
    expect(wrapper.get('[role="status"]').text()).toBe('No pings yet.');
  });

  it('adds a ping with the typed note, then reloads the list', async () => {
    const bodies: unknown[] = [];
    const fetch = stubApi([first], (body) => bodies.push(body));
    const wrapper = render();
    await flushPromises();
    await wrapper.get('input').setValue('  hello  ');
    await wrapper.get('form').trigger('submit');
    await flushPromises();
    expect(bodies).toEqual([{ note: 'hello' }]);
    expect(fetch.mock.calls.filter(([url]) => String(url) === '/api/pings')).toHaveLength(3);
    expect((wrapper.get('input').element as HTMLInputElement).value).toBe('');
  });

  it('shows an HTTP failure without pretending it is a list', async () => {
    stubApi(new Response('Unavailable', { status: 503 }));
    const wrapper = render();
    await flushPromises();
    expect(wrapper.get('[role="alert"]').text()).toBe('Error: API returned 503');
    expect(wrapper.find('li').exists()).toBe(false);
  });

  it('rejects a malformed successful response', async () => {
    stubApi([{ id: 1, note: 42 }]);
    const wrapper = render();
    await flushPromises();
    expect(wrapper.get('[role="alert"]').text()).toContain('Error:');
    expect(wrapper.text()).not.toContain('42');
  });

  it('aborts its requests when the page is removed', async () => {
    const signals: (AbortSignal | null | undefined)[] = [];
    vi.stubGlobal(
      'fetch',
      vi.fn((_url: RequestInfo | URL, init?: RequestInit) => {
        signals.push(init?.signal);
        return new Promise<Response>(() => {});
      }),
    );
    const wrapper = render();
    await flushPromises();
    expect(signals).toHaveLength(2);
    expect(signals.every((signal) => signal?.aborted === false)).toBe(true);
    wrapper.unmount();
    wrappers.splice(wrappers.indexOf(wrapper), 1);
    expect(signals.every((signal) => signal?.aborted === true)).toBe(true);
    await flushPromises();
  });
});
