import { ChangeDetectionStrategy, Component, computed, inject } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { map } from 'rxjs';

@Component({
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [RouterLink],
  selector: 'ez-catalog-server-error',
  styleUrl: './server-error.css',
  templateUrl: './server-error.html',
})
export class ServerError {
  private readonly route = inject(ActivatedRoute);
  private readonly statusParam = toSignal(this.route.paramMap.pipe(map((params) => params.get('status'))));

  status = computed(() => {
    const status = Number(this.statusParam());
    return Number.isInteger(status) && status >= 500 && status <= 599 ? status : 500;
  });
}
