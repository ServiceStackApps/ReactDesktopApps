import * as React from 'react';

const View = (props) => (
    <div>
        <h3>{props.message}</h3>
    </div>
);

export default () => <View message="This is View 2" />;
