// Licensed to the Software Freedom Conservancy (SFC) under one
// or more contributor license agreements.  See the NOTICE file
// distributed with this work for additional information
// regarding copyright ownership.  The SFC licenses this file
// to you under the Apache License, Version 2.0 (the
// "License"); you may not use this file except in compliance
// with the License.  You may obtain a copy of the License at
//
//   http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing,
// software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
// KIND, either express or implied.  See the License for the
// specific language governing permissions and limitations
// under the License.

use crate::logger::Logger;
use anyhow::Error;
use std::cell::RefCell;
use std::fs::File;
use std::path::{Path, PathBuf};

use crate::files::{create_parent_path_if_not_exists, create_path_if_not_exists};
use fs2::FileExt;
use std::fs;

thread_local!(static LOCK_PATH: RefCell<Option<PathBuf>> = RefCell::new(None));

const LOCK_FILE: &str = "sm.lock";

pub struct Lock {
    file: File,
    path: PathBuf,
}

impl Lock {
    // Acquire file lock to prevent race conditions accessing the cache folder by concurrent SM processes
    pub fn acquire(
        log: &Logger,
        target: &Path,
        single_file: Option<String>,
    ) -> Result<Self, Error> {
        let lock_folder = if single_file.is_some() {
            create_parent_path_if_not_exists(target)?;
            target.parent().unwrap()
        } else {
            create_path_if_not_exists(target)?;
            target
        };
        let path = lock_folder.join(LOCK_FILE);
        let file = File::create(&path)?;

        log.debug(format!("Acquiring lock: {}", path.display()));
        file.lock_exclusive().unwrap_or_default();

        LOCK_PATH.with(|option| {
            *option.borrow_mut() = Some(lock_folder.to_path_buf());
        });

        Ok(Self { file, path })
    }

    pub fn release(&mut self) {
        fs::remove_file(&self.path).unwrap_or_default();
        self.file.unlock().unwrap_or_default();

        LOCK_PATH.with(|option| {
            *option.borrow_mut() = None;
        });
    }

    pub fn exists(&mut self) -> bool {
        self.path.exists()
    }
}

pub fn clear_lock_if_required() {
    let mut lock_path: Option<PathBuf> = None;
    LOCK_PATH.with(|option| {
        let optional_path = &*option.borrow();
        if optional_path.is_some() {
            lock_path = Some(optional_path.as_ref().unwrap().to_path_buf());
        }
    });
    if lock_path.is_some() {
        fs::remove_dir_all(lock_path.unwrap()).unwrap_or_default();
    }
}
