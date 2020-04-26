mkdir mobirise-template
cd mobirise-template
copy NUL index.html
mkdir assets
cd ..
ng new mobirise-angular-integration & cd mobirise-angular-integration & ng g c main-container & cd src & cd app & mkdir mobirise-components & break>app.component.html & break>app-routing.module.ts & ( 
  echo import { NgModule } from '@angular/core'; 
  echo import { Routes, RouterModule } from '@angular/router';
  echo import { MainContainerComponent } from './main-container/main-container.component';
  echo const routes: Routes = [{ path: '**', component: MainContainerComponent }];
  echo @NgModule({
  echo imports: ^[RouterModule.forRoot^(routes^)^],
  echo exports: [RouterModule],
  echo ^}^)
  echo export class AppRoutingModule {}
) > app-routing.module.ts & (
  echo ^<router-outlet^>^<^/router-outlet^>
) > app.component.html