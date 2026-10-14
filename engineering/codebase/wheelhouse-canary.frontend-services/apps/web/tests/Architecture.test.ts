import { ESLint } from 'eslint';
import { describe, expect, it } from 'vitest';

const eslint = new ESLint();

describe('application boundaries', () => {
  it('rejects a presentation import from application orchestration', async () => {
    const [result] = await eslint.lintText(
      "import Page from '../../presentation/pings/pages/PingsPage.vue'; void Page;",
      { filePath: 'src/application/pings/Invalid.ts' },
    );
    expect(result?.messages.some((message) => message.ruleId === 'no-restricted-imports')).toBe(true);
  });

  it('allows the pings integration seam', async () => {
    const [result] = await eslint.lintText("import { pingsApi } from '../../integration/pings'; void pingsApi;", {
      filePath: 'src/application/pings/Valid.ts',
    });
    expect(result?.messages).toEqual([]);
  });
});
