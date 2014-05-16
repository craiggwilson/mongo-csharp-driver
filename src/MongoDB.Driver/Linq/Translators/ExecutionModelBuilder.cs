﻿/* Copyright 2010-2012 10gen Inc.
*
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
*
* http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
*/

using System;
using System.Linq.Expressions;
using MongoDB.Driver.Linq.Expressions;

namespace MongoDB.Driver.Linq.Translators
{
    internal class ExecutionModelBuilder : LinqToMongoExpressionVisitor
    {
        // private fields
        private ExecutionModel _executionModel;
        private ExecutionTarget _executionTarget;
        private Exception _lastException;

        // public methods
        public ExecutionModel Build(Expression node, ExecutionTarget executionTarget)
        {
            _executionTarget = executionTarget;
            Visit(node);

            if (_executionModel == null)
            {
                throw LinqErrors.Unsupported(node, _executionTarget, _lastException);
            }

            return _executionModel;
        }

        // protected methods
        protected override Expression VisitPipeline(PipelineExpression node)
        {
            if ((_executionTarget & ExecutionTarget.Query) == ExecutionTarget.Query)
            {
                try
                {
                    _executionModel = new QueryModelBuilder().Build(node);
                    if (_executionModel != null)
                    {
                        return node;
                    }
                }
                catch (MongoLinqException ex)
                {
                    _lastException = ex;
                }
                catch (NotSupportedException ex)
                {
                    _lastException = ex;
                }
            }

            if ((_executionTarget & ExecutionTarget.Pipeline) == ExecutionTarget.Pipeline)
            {
                try
                {
                    _executionModel = new PipelineModelBuilder().Build(node);
                }
                catch (MongoLinqException ex)
                {
                    _lastException = ex;
                }
                catch (NotSupportedException ex)
                {
                    _lastException = ex;
                }
            }

            return node;
        }
    }
}