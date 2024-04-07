const QuickSearchFilter = (props) => {
    console.log(props)
    const react = window.ui.getSystem().React;

    const propertyInputClasses = {
        type: 'text',
        placeholder: `${props.description}.Property`
    }

    const filterInputClasses = {
        type: 'text',
        placeholder: `${props.description}.Filter`
    }

    const valueInputClasses = {
        type: 'text',
        placeholder: `${props.description}.Value`
    }

    const inputs = [
        react.createElement('input', propertyInputClasses),
        react.createElement('input', filterInputClasses),
        react.createElement('input', valueInputClasses),
    ]

    return react.createElement(
        'div',
        null,
        inputs,
    );

}

const QuickSearchFilterSwaggerPlugin = {
    components: {
        JsonSchema_object_quicksearchfilter: QuickSearchFilter
    }
};

window.swaggerPlugins = [
    QuickSearchFilterSwaggerPlugin
]