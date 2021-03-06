// Copyright (c) Microsoft. All rights reserved.

import React, { Component } from 'react';

import { permissions, toDiagnosticsModel } from 'services/models';
import { UsersGridContainer } from './usersGrid';
import {
  AjaxError,
  Btn,
  ComponentArray,
  ContextMenu,
  ContextMenuAlign,
  PageContent,
  PageTitle,
  Protected,
  RefreshBarContainer as RefreshBar,
  SearchInput
} from 'components/shared';
import { UserNewContainer } from './flyouts/userNew';
import { UserNewServicePrincipalContainer } from './flyouts/userNewServicePrincipal/userNewServicePrincipal.container';
import { svgs } from 'utilities';

import './users.scss';

const closedFlyoutState = { openFlyoutName: undefined };

export class Users extends Component {

  constructor(props) {
    super(props);
    this.state = {
      ...closedFlyoutState,
      contextBtns: null
    };

    this.props.updateCurrentWindow('Users');
  }

  componentWillReceiveProps(nextProps) {
    if (nextProps.isPending && nextProps.isPending !== this.props.isPending) {
      // If the grid data refreshes, hide the flyout and deselect soft selections
      this.setState(closedFlyoutState);
    }
  }

  closeFlyout = () => this.setState(closedFlyoutState);

  openNewUserFlyout = () => {
    this.setState({ openFlyoutName: 'new-user' });
    this.props.logEvent(toDiagnosticsModel('Users_NewClick', {}));
  }
  openNewServicePrincipalFlyout = () => {
    this.setState({ openFlyoutName: 'new-sp' });
    this.props.logEvent(toDiagnosticsModel('ServicePrincipal_NewClick', {}));
  }

  onContextMenuChange = contextBtns => this.setState({
    contextBtns,
    openFlyoutName: undefined
  });

  onGridReady = gridReadyEvent => {
    this.userGridApi = gridReadyEvent.api
    if(!this.props.users || this.props.users.length == 0){
      this.props.fetchUsers() // Load Users for the first time
    }

  };

  searchOnChange = ({ target: { value } }) => {
    if (this.userGridApi) this.userGridApi.setQuickFilter(value);
  };

  onSearchClick = () => {
    this.props.logEvent(toDiagnosticsModel('Users_Search', {}));
  };

  render() {
    const { t, users, userGroupError, userError, isPending, lastUpdated, fetchUsers } = this.props;

    const gridProps = {
      onGridReady: this.onGridReady,
      rowData: isPending ? undefined : users || [],
      onContextMenuChange: this.onContextMenuChange,
      t: this.props.t
    };
    const newUserFlyoutOpen = this.state.openFlyoutName === 'new-user';
    const newServicePrincipalFlyoutOpen = this.state.openFlyoutName === 'new-sp';

    const error = userGroupError || userError;

    return (
      <ComponentArray>
        <ContextMenu>
          <ContextMenuAlign>
            <SearchInput
            onChange={this.searchOnChange}
            onClick={this.onSearchClick}
            aria-label={t('users.ariaLabel')}
            placeholder={t('users.searchPlaceholder')} />
            {this.state.contextBtns}
            <Protected permission={permissions.inviteUsers}>
              <Btn svg={svgs.plus} onClick={this.openNewUserFlyout}>{t('users.flyouts.new.contextMenuName')}</Btn>
            </Protected>
            <Protected permission={permissions.inviteUsers}>
              <Btn svg={svgs.plus} onClick={this.openNewServicePrincipalFlyout}>{t('users.flyouts.new.addServicePrincipal')}</Btn>
            </Protected>
            <RefreshBar refresh={fetchUsers} time={lastUpdated} isPending={isPending} t={t} />
          </ContextMenuAlign>
        </ContextMenu>
        <PageContent className="users-container">
          <PageTitle titleValue={t('users.title')} />
          {!!error && <AjaxError t={t} error={error} />}
          {!error && <UsersGridContainer {...gridProps} />}
          {newUserFlyoutOpen && <UserNewContainer onClose={this.closeFlyout} />}
          {newServicePrincipalFlyoutOpen && <UserNewServicePrincipalContainer onClose={this.closeFlyout} />}
        </PageContent>
      </ComponentArray>
    );
  }
}
