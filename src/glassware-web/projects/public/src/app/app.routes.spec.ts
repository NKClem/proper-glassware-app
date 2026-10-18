import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { RouterTestingHarness } from '@angular/router/testing';
import { routes } from './app.routes';
import { NotFound } from './features/not-found/not-found';

describe('public app routes', () => {
  beforeEach(() => {
    TestBed.configureTestingModule({ providers: [provideRouter(routes)] });
  });

  it.each(['/', '/some-slug/glasses/5', '/no/such/page'])(
    'shows the Not found page for unknown URL %s',
    async (url) => {
      const harness = await RouterTestingHarness.create();

      const component = await harness.navigateByUrl(url);

      expect(component).toBeInstanceOf(NotFound);
      expect(harness.routeNativeElement?.textContent).toContain('Not found');
    },
  );
});
