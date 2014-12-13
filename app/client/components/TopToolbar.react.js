var mui = require('material-ui');
var RaisedButton = mui.RaisedButton;
var React = require('react');

var TopToolbar = React.createClass({

  render: function() {

    var filterOptions = [
        {
          payload: '1',
          text: 'All Broadcasts'
        },
        {
          payload: '2',
          text: 'All Voice'
        },
        {
          payload: '3',
          text: 'All Text'
        },
        {
          payload: '4',
          text: 'Complete Voice'
        },
        {
          payload: '5',
          text: 'Complete Text'
        },
        {
          payload: '6',
          text: 'Active Voice'
        },
        {
          payload: '7',
          text: 'Active Text'
        },
                  ],
      iconMenuItems = [
        {
          payload: '1',
          text: 'Download'
        },
        {
          payload: '2',
          text: 'More Info'
        }
                      ];

    return (


        <mui.Toolbar>
          <mui.ToolbarGroup key={0} float="left">
          <mui.DropDownMenu menuItems={filterOptions} />
          </mui.ToolbarGroup>
          <mui.ToolbarGroup key={1} float="right">
          <mui.Icon icon='mui-icon-pie' />
          <mui.Icon icon='mui-icon-sort' />
          <mui.DropDownIcon icon="navigation-expand-more" menuItems={iconMenuItems}
          onChange={this._onDropDownMenuChange} />
          <span className="mui-toolbar-separator">&nbsp;</span>
          <mui.RaisedButton label="Create Broadcast" primary={true} />
          </mui.ToolbarGroup>
        </mui.Toolbar>
      

    );
  },

  _onDropDownMenuChange: function(e, key, menuItem) {
    console.log('Menu Clicked: ', menuItem);
  },

});

module.exports = TopToolbar;
