// Copyright (c) Microsoft. All rights reserved.

import { stringify } from 'query-string';
import Config from 'app.config';
import { HttpClient } from 'utilities/httpClient';
import { Observable } from 'rxjs';
import {
  toActiveAlertsModel,
  toAlertForRuleModel,
  toAlertsForRuleModel,
  toAlertsModel,
  toMessagesModel,
  toRuleModel,
  toRulesModel,
  toStatusModel,
  toTelemetryRequestModel
} from './models';
import { Alert } from '@microsoft/azure-iot-ux-fluent-controls';

const ENDPOINT = Config.serviceUrls.telemetry;

/** Contains methods for calling the telemetry service */
export class TelemetryService {

  /** Returns the status properties for the telemetry service */
  static getStatus() {
    return HttpClient.get(`${ENDPOINT}status`)
      .map(toStatusModel);
  }

  /** Returns a list of rules */
  static getRules(params = {}) {
    return HttpClient.get(`${ENDPOINT}rules?${stringify(params)}`)
      .map(toRulesModel);
  }

  /** creates a new rule */
  static createRule(rule) {
    return HttpClient.post(`${ENDPOINT}rules`, rule)
      .map(toRuleModel);
  }

  /** updates an existing rule */
  static updateRule(id, rule) {
    return HttpClient.put(`${ENDPOINT}rules/${id}`, rule)
      .map(toRuleModel);
  }

  /** Returns a list of alarms (all statuses) */
  static getAlerts(params = {}) {
    if (params.devices && !Array.isArray(params.devices)) {
      params.devices = params.devices.split(",");
    }
    var body = toTelemetryRequestModel(params);
    return HttpClient.post(`${ENDPOINT}alarms`, body)
        .catch(error => this.catch404(error))
        .map(toAlertsModel);
  }

  /** Returns a list of active alarms (open or ack) */
  static getActiveAlerts(params = {}) {
    if (params.devices && !Array.isArray(params.devices)) {
      params.devices = params.devices.split(",");
    }
    var body = toTelemetryRequestModel(params);
    return HttpClient.post(`${ENDPOINT}alarmsbyrule`, body)
      .catch(error => this.catch404(error))
      .map(toActiveAlertsModel);
  }

  /** Returns a list of alarms created from a given rule */
  static getAlertsForRule(id, params = {}) {
    if (params.devices && !Array.isArray(params.devices)) {
      params.devices = params.devices.split(",");
    }
    var body = toTelemetryRequestModel(params);
    return HttpClient.post(`${ENDPOINT}alarmsbyrule/${id}`, body)
        .catch(error => this.catch404(error))
        .map(toAlertsForRuleModel);
  }

  /** Returns a list of alarms created from a given rule */
  static updateAlertStatus(id, Status) {
    return HttpClient.patch(`${ENDPOINT}alarms/${encodeURIComponent(id)}`, { Status })
        .map(toAlertForRuleModel);
  }

  static deleteAlerts(ids) {
    const request = { Items: ids };
    return HttpClient.post(`${ENDPOINT}alarms!delete`, request);
  }

  /** Returns a telemetry events */
  static getTelemetryByMessages(params = {}) {
    var body = toTelemetryRequestModel(params);
    return HttpClient.post(`${ENDPOINT}messages`, body)
        .catch(error => this.catch404(error))
        .map(toMessagesModel);
  }

  static getTelemetryByDeviceId(devices = [], timeInterval) {
    console.log(timeInterval);
    return TelemetryService.getTelemetryByMessages({
      from: 'NOW-'+ timeInterval,
      to: 'NOW',
      order: 'desc',
      devices
    });
  }

  static getTelemetryByDeviceIdP1M(devices = []) {
    return TelemetryService.getTelemetryByMessages({
      from: 'NOW-PT1M',
      to: 'NOW',
      order: 'desc',
      devices
    });
  }

  static getTelemetryByDeviceIdP15M(devices = []) {
    return TelemetryService.getTelemetryByMessages({
      from: 'NOW-PT15M',
      to: 'NOW',
      order: 'desc',
      devices
    });
  }

  static deleteRule(id) {
    return HttpClient.delete(`${ENDPOINT}rules/${id}`)
      .map(() => ({ deletedRuleId: id }));
  }

  /*
    a 404 is thrown by some device telemetry apis when a collection does not exist
    for instances where this is the case, we want to catch this 404 and simply return no data instead
  */
  static catch404(error) {
    return error.status == 404 ? Observable.of({ messages: {} }) : Observable.throw(error)
  }
}
