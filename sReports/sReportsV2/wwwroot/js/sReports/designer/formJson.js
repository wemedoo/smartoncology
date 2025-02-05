﻿var editorTree;
var editorCode;
var schema;
var formDefinition;
getSchema();

function showJsonEditor(json = jsonTest) {
    var ajv = new Ajv({
        allErrors: true,
        verbose: true,
        schemaId: 'auto'
    });
    $('#jsoneditor').html('');
    $('.form-item').removeClass('active');

    editorTree = new JSONEditor(document.getElementById('jsoneditorTree'), {
        ajv: ajv,
        mode: 'tree',
        onChangeText: function (jsonString) {
            try {
                editorTree.updateText(jsonString);
                $('.jsoneditor-format').click();
            }
            catch (exception) {
                console.log(exception);
            }
        },
        onError: function (error) {
            console.log(error);
        }
    });

    // create editor 2
    editorCode = new JSONEditor(document.getElementById('jsoneditorCode'), {
        ajv: ajv,
        mode: 'code',
        onChangeText: function (jsonString) {
            try {
                editorTree.updateText(jsonString);
            }
            catch (exception) {
                console.log(exception);
            }

        },
        onError: function (error) {
            console.log(error);
        }
    });

    editorTree.set(json);
    editorCode.set(json);
    console.log(editorTree.get());
    $('#jsoneditorContainer').show();
}
function getFormTree(formDefinition) {
    setIsReadOnlyViewModeInRequest(formDefinition);
    callServer({
        method: 'post',
        data: formDefinition,
        url: `/Form/GetFormTree`,
        contentType: 'application/json',
        success: function (data) {
            $('#formTreeContainer').html(data);
        },
        error: function (xhr, textStatus, thrownError) {
            handleResponseError(xhr);
        }
    });
}

function goBackToTree() {
    formDefinition = editorTree.get();
    getFormTree(formDefinition);
    getNestableFormElements();
}


function cancelJsonEditor() {
    window.location.href = '/Form/GetAll';
}

function getSchema() {
    schema = schemaJson;
} 

var schemaJson = {
    'id': 'http://json-schema.org/draft-04/schema#',
    '$schema': 'http://json-schema.org/draft-04/schema#',
    'description': 'Core schema meta-schema',
    'definitions': {
        'schemaArray': {
            'type': 'array',
            'minItems': 1,
            'items': { '$ref': '#' }
        },
        'positiveInteger': {
            'type': 'integer',
            'minimum': 0
        },
        'positiveIntegerDefault0': {
            'allOf': [{ '$ref': '#/definitions/positiveInteger' }, { 'default': 0 }]
        },
        'simpleTypes': {
            'enum': ['array', 'boolean', 'integer', 'null', 'number', 'object', 'string']
        },
        'stringArray': {
            'type': 'array',
            'items': { 'type': 'string' },
            'minItems': 1,
            'uniqueItems': true
        }
    },
    'type': 'object',
    'properties': {
        'id': {
            'type': 'string'
        },
        '$schema': {
            'type': 'string'
        },
        'title': {
            'type': 'string'
        },
        'description': {
            'type': 'string'
        },
        'default': {},
        'multipleOf': {
            'type': 'number',
            'minimum': 0,
            'exclusiveMinimum': true
        },
        'maximum': {
            'type': 'number'
        },
        'exclusiveMaximum': {
            'type': 'boolean',
            'default': false
        },
        'minimum': {
            'type': 'number'
        },
        'exclusiveMinimum': {
            'type': 'boolean',
            'default': false
        },
        'maxLength': { '$ref': '#/definitions/positiveInteger' },
        'minLength': { '$ref': '#/definitions/positiveIntegerDefault0' },
        'pattern': {
            'type': 'string',
            'format': 'regex'
        },
        'additionalItems': {
            'anyOf': [
                { 'type': 'boolean' },
                { '$ref': '#' }
            ],
            'default': {}
        },
        'items': {
            'anyOf': [
                { '$ref': '#' },
                { '$ref': '#/definitions/schemaArray' }
            ],
            'default': {}
        },
        'maxItems': { '$ref': '#/definitions/positiveInteger' },
        'minItems': { '$ref': '#/definitions/positiveIntegerDefault0' },
        'uniqueItems': {
            'type': 'boolean',
            'default': false
        },
        'maxProperties': { '$ref': '#/definitions/positiveInteger' },
        'minProperties': { '$ref': '#/definitions/positiveIntegerDefault0' },
        'required': { '$ref': '#/definitions/stringArray' },
        'additionalProperties': {
            'anyOf': [
                { 'type': 'boolean' },
                { '$ref': '#' }
            ],
            'default': {}
        },
        'definitions': {
            'type': 'object',
            'additionalProperties': { '$ref': '#' },
            'default': {}
        },
        'properties': {
            'type': 'object',
            'additionalProperties': { '$ref': '#' },
            'default': {}
        },
        'patternProperties': {
            'type': 'object',
            'additionalProperties': { '$ref': '#' },
            'default': {}
        },
        'dependencies': {
            'type': 'object',
            'additionalProperties': {
                'anyOf': [
                    { '$ref': '#' },
                    { '$ref': '#/definitions/stringArray' }
                ]
            }
        },
        'enum': {
            'type': 'array',
            'minItems': 1,
            'uniqueItems': true
        },
        'type': {
            'anyOf': [
                { '$ref': '#/definitions/simpleTypes' },
                {
                    'type': 'array',
                    'items': { '$ref': '#/definitions/simpleTypes' },
                    'minItems': 1,
                    'uniqueItems': true
                }
            ]
        },
        'format': { 'type': 'string' },
        'allOf': { '$ref': '#/definitions/schemaArray' },
        'anyOf': { '$ref': '#/definitions/schemaArray' },
        'oneOf': { '$ref': '#/definitions/schemaArray' },
        'not': { '$ref': '#' }
    },
    'dependencies': {
        'exclusiveMaximum': ['maximum'],
        'exclusiveMinimum': ['minimum']
    },
    'default': {}
};
