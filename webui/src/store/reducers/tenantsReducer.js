// Copyright (c) Microsoft. All rights reserved.

import 'rxjs';
import { Observable } from 'rxjs';
import moment from 'moment';
import { schema, normalize } from 'normalizr';
import update from 'immutability-helper';
import { createSelector } from 'reselect';
import { redux as appRedux, getActiveDeviceGroupConditions } from './appReducer';
import { TenantService } from 'services';
import {
  createReducerScenario,
  createEpicScenario,
  resetPendingAndErrorReducer,
  errorPendingInitialState,
  pendingReducer,
  errorReducer,
  setPending,
  toActionCreator,
  getPending,
  getError
} from 'store/utilities';

// ========================= Epics - START
const handleError = fromAction => error =>
  Observable.of(redux.actions.registerError(fromAction.type, { error, fromAction }));

export const epics = createEpicScenario({
  /** Load all tenants for the user */
  fetchTenants: {
    type: 'TENANTS_FETCH',
    epic: (fromAction, store) => {
      return TenantService.getAllTenants()
        .map(toActionCreator(redux.actions.updateTenants, fromAction))
        .catch(handleError(fromAction))
    }
  }
});

const tenantSchema = new schema.Entity('tenants');
const tenantListSchema = new schema.Array(tenantSchema);
const initialState = { ...errorPendingInitialState, entities: {}, items: [], lastUpdated: '' };

const updateTenantsReducer = (state, { payload, fromAction }) => {
  const { entities: { tenants }, result } = normalize(payload, tenantListSchema);
  return update(state, {
    entities: { $set: tenants },
    items: { $set: result },
    lastUpdated: { $set: moment() },
    ...setPending(fromAction.type, false)
  });
};

const insertTenantReducer = (state, { payload }) => {
  const inserted = payload.map(user => ({ ...user, isNew: true }));
  const { entities: { tenants}, result } = normalize(inserted, tenantListSchema);
  if (state.entities) {
    return update(state, {
      entities: { $merge: tenants },
      items: { $splice: [[0, 0, ...result]] }
    });
  }
  return update(state, {
    entities: { $set: tenants },
    items: { $set: result }
  });
};

const fetchableTypes = [
  epics.actionTypes.fetchTenants
];

export const redux = createReducerScenario({
  updateTenants: { type: 'TENANTS_UPDATE', reducer: updateTenantsReducer },
  insertTenant: { type: 'TENANT_INSERT', reducer: insertTenantReducer },
  registerError: { type: 'TENANT_REDUCER_ERROR', reducer: errorReducer },
  //isFetching: { multipleType: fetchableTypes, reducer: pendingReducer },
  resetPendingAndError: {type: 'TENANT_REDUCER_RESET_ERROR_PENDING', reducer: resetPendingAndErrorReducer }
});

export const reducer = { tenants: redux.getReducer(initialState) };

export const getTenantsReducer = state => state.tenants;
export const getEntities = state => getTenantsReducer(state).entities || {};
export const getTenantsLastUpdated = state => getTenantsReducer(state).lastUpdated;
export const getItems = state => getTenantsReducer(state).items || [];
export const getTenantsError = state => getError(getTenantsReducer(state), epics.actionTypes.fetchTenants);
export const getTenantsPendingStatus = state => getPending(getTenantsReducer(state), epics.actionTypes.fetchTenants);
export const getTenantById = (state, id) => getEntities(state)[id];
export const getTenants = createSelector(getEntities, getItems, (entities, items) => {
  return items.map(id => entities[id])
});
